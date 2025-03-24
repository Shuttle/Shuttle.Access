using System.Data.Common;
using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using Serilog;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;
using Shuttle.Esb.AzureStorageQueues;
using Shuttle.Esb.Sql.Subscription;
using Shuttle.OAuth;
using Shuttle.Recall;
using Shuttle.Recall.Sql.EventProcessing;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);

        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        var configurationFolder = Environment.GetEnvironmentVariable("CONFIGURATION_FOLDER");

        if (string.IsNullOrEmpty(configurationFolder))
        {
            throw new ApplicationException("Environment variable `CONFIGURATION_FOLDER` has not been set.");
        }

        var appsettingsPath = Path.Combine(configurationFolder, "appsettings.json");

        if (!File.Exists(appsettingsPath))
        {
            throw new ApplicationException($"File '{appsettingsPath}' cannot be accessed/found.");
        }

        var webApplicationBuilder = WebApplication.CreateBuilder(args);

        webApplicationBuilder.Host.UseSerilog();

        webApplicationBuilder.Configuration
            .AddJsonFile(appsettingsPath);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(webApplicationBuilder.Configuration)
            .CreateLogger();

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
            .AddAuthentication(options =>
            {
                options.DefaultScheme = "Routing";
            })
            .AddScheme<AuthenticationSchemeOptions, RoutingAuthenticationHandler>(RoutingAuthenticationHandler.AuthenticationScheme, _ =>
            {
            })
            .AddScheme<AuthenticationSchemeOptions, JwtBearerAuthenticationHandler>(JwtBearerAuthenticationHandler.AuthenticationScheme, _ =>
            {
            })
            .AddScheme<AuthenticationSchemeOptions, AccessAuthenticationHandler>(AccessAuthenticationHandler.AuthenticationScheme, _ =>
            {
            });

        webApplicationBuilder.Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => type.FullName);

                options.AddSecurityDefinition("Bearer", new()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "Token",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer TOKEN', where 'TOKEN' is a JWT; else 'Shuttle.Access token=TOKEN', where 'TOKEN' is the Shuttle.Access GUID session token."
                });

                options.AddSecurityRequirement(new()
                {
                    {
                        new()
                        {
                            Reference = new()
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        []
                    }
                });
            })
            .AddDataAccess(builder =>
            {
                builder.AddConnectionString("Access", "Microsoft.Data.SqlClient");
                builder.Options.DatabaseContextFactory.DefaultConnectionStringName = "Access";
            })
            .AddSingleton<IHashingService, HashingService>()
            .AddSingleton<IPasswordGenerator, DefaultPasswordGenerator>()
            .AddAccess(builder =>
            {
                webApplicationBuilder.Configuration.GetSection(AccessOptions.SectionName).Bind(builder.Options);
            })
            .AddSqlSubscription(builder =>
            {
                builder.Options.ConnectionStringName = "Access";

                builder.UseSqlServer();
            })
            .AddServiceBus(builder =>
            {
                webApplicationBuilder.Configuration.GetSection(ServiceBusOptions.SectionName).Bind(builder.Options);

                builder.AddSubscription<IdentityRoleSet>();
                builder.AddSubscription<RolePermissionSet>();
                builder.AddSubscription<PermissionStatusSet>();
            })
            .AddAzureStorageQueues(builder =>
            {
                var queueOptions = webApplicationBuilder.Configuration.GetSection($"{AzureStorageQueueOptions.SectionName}:Access").Get<AzureStorageQueueOptions>() ?? new();

                if (string.IsNullOrWhiteSpace(queueOptions.StorageAccount))
                {
                    queueOptions.ConnectionString = webApplicationBuilder.Configuration.GetConnectionString("azure") ?? string.Empty;
                }

                builder.AddOptions("azure", queueOptions);
            })
            .AddEventStore()
            .AddSqlEventStorage(builder =>
            {
                builder.Options.ConnectionStringName = "Access";

                builder.UseSqlServer();
            })
            .AddSqlEventProcessing(builder =>
            {
                builder.Options.ConnectionStringName = "Access";

                builder.UseSqlServer();
            })
            .AddMediator(builder =>
            {
                builder.AddParticipants(Assembly.Load("Shuttle.Access.Application"));
            })
            .AddAccessAuthorization()
            .AddSqlAccess()
            .AddDataStoreAccessService()
            .AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            })
            .AddOAuth(builder =>
            {
                webApplicationBuilder.Configuration.GetSection(OAuthOptions.SectionName).Bind(builder.Options);

                Log.Debug($"[oauth] : DefaultRedirectUri = '{builder.Options.DefaultRedirectUri}'");

                foreach (var optionsProvider in builder.Options.Providers)
                {
                    Log.Debug($"[oauth:{optionsProvider.Name}] : Scope = '{optionsProvider.Scope}'");
                    Log.Debug($"[oauth:{optionsProvider.Name}.Issuer] : Uri = '{optionsProvider.Issuer.Uri}'");
                    Log.Debug($"[oauth:{optionsProvider.Name}.Authorize] : ClientId = '{optionsProvider.Authorize.ClientId}' / CodeChallengeMethod = '{optionsProvider.Authorize.CodeChallengeMethod}' / Url = '{optionsProvider.Authorize.Url}'");
                    Log.Debug($"[oauth:{optionsProvider.Name}.Token] : ClientId = '{optionsProvider.Token.ClientId}' / ContentTypeHeader = '{optionsProvider.Token.ContentTypeHeader}' / OriginHeader = '{optionsProvider.Token.OriginHeader}' / Url = '{optionsProvider.Token.Url}'");
                    Log.Debug($"[oauth:{optionsProvider.Name}.Data] : AuthorizationHeaderScheme = '{optionsProvider.Data.AuthorizationHeaderScheme}' / AcceptHeader = '{optionsProvider.Data.AcceptHeader}' / EMailPropertyName = '{optionsProvider.Data.EMailPropertyName}' / Url = '{optionsProvider.Data.Url}'");
                }
            })
            .AddInMemoryOAuthGrantRepository();

        var app = webApplicationBuilder.Build();

        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(apiVersion1)
            .ReportApiVersions()
            .Build();

        app.UseCors("AllowAll");
        app.UseAccessAuthorization();

        app
            .MapApplicationEndpoints(versionSet)
            .MapIdentityEndpoints(versionSet)
            .MapOAuthEndpoints(versionSet)
            .MapPermissionEndpoints(versionSet)
            .MapRoleEndpoints(versionSet)
            .MapServerEndpoints(versionSet)
            .MapSessionEndpoints(versionSet)
            .MapStatisticEndpoints(versionSet);

        app
            .UseSwagger()
            .UseSwaggerUI();

        await app.RunAsync();
    }
}