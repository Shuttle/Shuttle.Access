using System.Data.Common;
using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;
using Shuttle.Esb.AzureStorageQueues;
using Shuttle.Esb.Sql.Subscription;
using Shuttle.Recall;
using Shuttle.Recall.Sql.EventProcessing;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);

        var webApplicationBuilder = WebApplication.CreateBuilder(args);

        var apiVersion1 = new ApiVersion(1, 0);

        webApplicationBuilder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = apiVersion1;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        webApplicationBuilder.Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddDataAccess(builder =>
            {
                builder.AddConnectionString("Access", "Microsoft.Data.SqlClient");
                builder.Options.DatabaseContextFactory.DefaultConnectionStringName = "Access";
            })
            .AddSingleton<IHashingService, HashingService>()
            .AddSingleton<IPasswordGenerator, DefaultPasswordGenerator>()
            .AddAccess()
            .AddSqlAccess()
            .AddSqlSubscription()
            .AddServiceBus(builder =>
            {
                webApplicationBuilder.Configuration.GetSection(ServiceBusOptions.SectionName).Bind(builder.Options);

                builder.Options.Subscription.ConnectionStringName = "Access";

                builder.AddSubscription<IdentityRoleSet>();
                builder.AddSubscription<RolePermissionSet>();
                builder.AddSubscription<PermissionStatusSet>();
            })
            .AddAzureStorageQueues(builder =>
            {
                builder.AddOptions("azure", new AzureStorageQueueOptions
                {
                    ConnectionString = webApplicationBuilder.Configuration.GetConnectionString("azure")
                });
            })
            .AddEventStore()
            .AddSqlEventStorage(builder =>
            {
                builder.Options.ConnectionStringName = "Access";
            })
            .AddSqlEventProcessing(builder =>
            {
                builder.Options.ConnectionStringName = "Access";
            })
            .AddMediator(builder =>
            {
                builder.AddParticipants(Assembly.Load("Shuttle.Access.Application"));
            })
            .AddAccessAuthorization();

        var app = webApplicationBuilder.Build();

        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(apiVersion1)
            .ReportApiVersions()
            .Build();

        app.UseAccessAuthorization();

        // server

        app.MapGet("/v{version:apiVersion}/server/configuration", (HttpContext _) =>
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);

                return new ServerConfiguration { Version = $"{version.Major}.{version.Minor}.{version.Build}" };
            })
            .WithTags("Server")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        // sessions

        app.MapPost("/v{version:apiVersion}/sessions", async (ISessionService sessionService, IDatabaseContextFactory databaseContextFactory, [FromBody] RegisterSession message) =>
            {
                if (string.IsNullOrEmpty(message.IdentityName) ||
                    (string.IsNullOrEmpty(message.Password) && Guid.Empty.Equals(message.Token)))
                {
                    return Results.BadRequest();
                }

                RegisterSessionResult registerSessionResult;

                await using (databaseContextFactory.Create())
                {
                    registerSessionResult = await sessionService.RegisterAsync(message.IdentityName, message.Password, message.Token);
                }

                return registerSessionResult.Ok
                    ? Results.Ok(new SessionRegistered
                    {
                        IdentityName = registerSessionResult.IdentityName,
                        Token = registerSessionResult.Token,
                        TokenExpiryDate = registerSessionResult.TokenExpiryDate,
                        Permissions = registerSessionResult.Permissions.ToList()
                    })
                    : Results.BadRequest();
            })
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapPost("/v{version:apiVersion}/sessions/delegated", async (HttpContext httpContext, IDatabaseContextFactory databaseContextFactory, ISessionService sessionService, RegisterDelegatedSession message) =>
            {
                if (string.IsNullOrEmpty(message.IdentityName))
                {
                    return Results.BadRequest();
                }

                var sessionTokenResult = httpContext.GetAccessSessionToken();

                if (!sessionTokenResult.Ok)
                {
                    return Results.Unauthorized();
                }

                RegisterSessionResult registerSessionResult;

                await using (databaseContextFactory.Create())
                {
                    registerSessionResult = await sessionService.RegisterAsync(message.IdentityName, sessionTokenResult.SessionToken);
                }

                if (!registerSessionResult.Ok)
                {
                    return Results.BadRequest();
                }

                return Results.Ok(new SessionRegistered
                {
                    IdentityName = registerSessionResult.IdentityName,
                    Token = registerSessionResult.Token,
                    TokenExpiryDate = registerSessionResult.TokenExpiryDate,
                    Permissions = registerSessionResult.Permissions.ToList()
                });
            })
            .WithTags("Sessions")
            .RequiresSession()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapDelete("/v{version:apiVersion}/sessions", async (HttpContext httpContext, IDatabaseContextFactory databaseContextFactory, ISessionService sessionService) =>
            {
                var sessionTokenResult = httpContext.GetAccessSessionToken();

                if (!sessionTokenResult.Ok)
                {
                    return Results.BadRequest();
                }

                await using (databaseContextFactory.Create())
                {
                    return await sessionService.RemoveAsync(sessionTokenResult.SessionToken)
                        ? Results.Ok()
                        : Results.Problem();
                }
            })
            .WithTags("Sessions")
            .RequiresPermission(Permissions.View.Sessions)
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/sessions/{token:Guid}", async (Guid token, IDatabaseContextFactory databaseContextFactory, ISessionQuery sessionQuery) =>
            {
                await using (databaseContextFactory.Create())
                {
                    var session = await sessionQuery.GetAsync(token);

                    return session == null
                        ? Results.BadRequest()
                        : Results.Ok(session);
                }
            })
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.View.Sessions);

        app.MapGet("/v{version:apiVersion}/sessions/{token:Guid}/permissions", async (Guid token, IDatabaseContextFactory databaseContextFactory, ISessionRepository sessionRepository) =>
            {
                await using (databaseContextFactory.Create())
                {
                    var session = await sessionRepository.FindAsync(token);

                    return session == null
                        ? Results.BadRequest()
                        : Results.Ok(session.Permissions);
                }
            })
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.View.Sessions);

        // roles

        app.MapPatch("/v{version:apiVersion}/roles/{id}/name", async (Guid id, [FromBody] SetRoleName message, [FromServices] IServiceBus serviceBus) =>
        {
            try
            {
                message.Id = id;
                message.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }

            await serviceBus.SendAsync(message);

            return Results.Accepted();
        })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.Register.Role);

        app.MapPatch("/v{version:apiVersion}/roles/{id}/permissions", async (Guid id, [FromBody] SetRolePermission message, [FromServices] IServiceBus serviceBus) =>
        {
            try
            {
                message.RoleId = id;
                message.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }

            await serviceBus.SendAsync(message);

            return Results.Accepted();
        })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.Register.Role);

        app.MapPost("/v{version:apiVersion}/roles/{id}/permissions/availability", async (Guid id, [FromBody] Identifiers<Guid> identifiers, [FromServices] IDatabaseContextFactory databaseContextFactory, [FromServices] IRoleQuery roleQuery) =>
        {
            try
            {
                identifiers.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }

            await using var context = databaseContextFactory.Create();

            var permissions = roleQuery.Permissions(new DataAccess.Query.Role.Specification().AddRoleId(id).AddPermissionIds(identifiers.Values)).ToList();

            return Results.Ok(from permissionId in identifiers.Values
                              select new IdentifierAvailability<Guid>()
                              {
                                  Id = permissionId,
                                  Active = permissions.Any(item => item.Id.Equals(permissionId))
                              });
        })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.Register.Role);

        app.MapGet("/v{version:apiVersion}/roles", async ([FromServices] IDatabaseContextFactory databaseContextFactory, [FromServices] IRoleQuery roleQuery) =>
        {
            await using var context = databaseContextFactory.Create();

            return Results.Ok(roleQuery.Search(new DataAccess.Query.Role.Specification()));
        })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.View.Role);

        app.MapGet("/v{version:apiVersion}/roles/{value}", async (string value, [FromServices] IDatabaseContextFactory databaseContextFactory, [FromServices] IRoleQuery roleQuery) =>
        {
            await using var context = databaseContextFactory.Create();

            var specification = new DataAccess.Query.Role.Specification();

            if (Guid.TryParse(value, out var id))
            {
                specification.AddRoleId(id);
            }
            else
            {
                specification.AddName(value);
            }

            var role = roleQuery
                .Search(specification.IncludePermissions())
                .FirstOrDefault();

            return role != null
                ? Results.Ok(role)
                : Results.BadRequest();
        })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.View.Role);

        app.MapDelete("/v{version:apiVersion}/roles/{id}", async (Guid id, [FromServices] IServiceBus serviceBus) =>
        {
            await serviceBus.SendAsync(new RemoveRole
            {
                Id = id
            });

            return Results.Accepted();
        })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.Remove.Role);

        app.MapPost("/v{version:apiVersion}/roles", async ([FromBody] RegisterRole message, [FromServices] IServiceBus serviceBus) =>
        {
            try
            {
                message.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }

            await serviceBus.SendAsync(message);

            return Results.Accepted();
        })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.Register.Role);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.Run();
    }
}