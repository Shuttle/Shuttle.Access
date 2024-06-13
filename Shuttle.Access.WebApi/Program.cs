using System.Data.Common;
using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.DataAccess;
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

        app.MapGet("/server/configuration", (HttpContext _) =>
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);

            return new { Version = $"{version.Major}.{version.Minor}.{version.Build}" };
        });

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
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        //app.MapPost("/v{version:apiVersion}/sessions/delegated", async (HttpContext httpContext, IDatabaseContextFactory databaseContextFactory, ISessionService sessionService, RegisterDelegatedSession message) =>
        //    {
        //        if (string.IsNullOrEmpty(message.IdentityName))
        //        {
        //            return Results.BadRequest();
        //        }

        //        var sessionTokenResult = httpContext.GetAccessSessionToken();

        //        if (!sessionTokenResult.Ok)
        //        {
        //            return Results.Unauthorized();
        //        }

        //        RegisterSessionResult registerSessionResult;

        //        await using (databaseContextFactory.Create())
        //        {
        //            registerSessionResult = await sessionService.RegisterAsync(message.IdentityName, sessionTokenResult.SessionToken);
        //        }

        //        if (!registerSessionResult.Ok)
        //        {
        //            return Results.BadRequest();
        //        }

        //        return Results.Ok(new SessionRegistered
        //        {
        //            IdentityName = registerSessionResult.IdentityName,
        //            Token = registerSessionResult.Token,
        //            TokenExpiryDate = registerSessionResult.TokenExpiryDate,
        //            Permissions = registerSessionResult.Permissions.ToList()
        //        });
        //    })
        //    .RequiresSession()
        //    .WithApiVersionSet(versionSet)
        //    .MapToApiVersion(apiVersion1);

        //app.MapDelete("/v{version:apiVersion}/sessions", async (HttpContext httpContext, IDatabaseContextFactory databaseContextFactory, ISessionService sessionService) =>
        //    {
        //        var sessionTokenResult = httpContext.GetAccessSessionToken();

        //        if (!sessionTokenResult.Ok)
        //        {
        //            return Results.BadRequest();
        //        }

        //        await using (databaseContextFactory.Create())
        //        {
        //            return await sessionService.RemoveAsync(sessionTokenResult.SessionToken)
        //                ? Results.Ok()
        //                : Results.Problem();
        //        }
        //    })
        //    .RequiresPermission(Permissions.View.Sessions)
        //    .WithApiVersionSet(versionSet)
        //    .MapToApiVersion(apiVersion1);

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
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.View.Sessions);

        //app.MapGet("/sessions/{token:Guid}/permissions", async (Guid token, IDatabaseContextFactory databaseContextFactory, ISessionRepository sessionRepository) =>
        //{
        //    await using (databaseContextFactory.Create())
        //    {
        //        var session = await sessionRepository.FindAsync(token);

        //        return session == null
        //            ? Results.BadRequest()
        //            : Results.Ok(session.Permissions);
        //    }
        //}).RequiresPermission(Permissions.View.Sessions);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.Run();
    }
}