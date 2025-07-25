using System.Data.Common;
using System.Reflection;
using Asp.Versioning;
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
            .AddJsonFile(appsettingsPath)
            .AddEnvironmentVariables();

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

        webApplicationBuilder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        webApplicationBuilder.Services
            .AddSingleton<IContextSessionService, NullContextSessionService>()
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => (type.FullName ?? string.Empty).Replace("+", "_"));

                options.AddSecurityDefinition("Bearer", new()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Custom",
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
            .AddAccessAuthorization(builder =>
            {
                webApplicationBuilder.Configuration.GetSection(AccessAuthorizationOptions.SectionName).Bind(builder.Options);

                builder.Options.PassThrough = false;
            })
            .AddSqlAccess()
            .AddDataStoreAccessService()
            .AddOAuth(builder =>
            {
                webApplicationBuilder.Configuration.GetSection(OAuthOptions.SectionName).Bind(builder.Options);
            })
            .AddInMemoryOAuthGrantRepository();

        var app = webApplicationBuilder.Build();

        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(apiVersion1)
            .ReportApiVersions()
            .Build();

        app.UseCors();
        app.UseSwagger();
        app.UseSwaggerUI();
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

        await app.RunAsync();
    }
}