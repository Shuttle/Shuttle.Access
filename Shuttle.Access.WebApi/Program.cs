using System.Data.Common;
using System.Reflection;
using Asp.Versioning;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.RestClient;
using Shuttle.Access.Sql;
using Shuttle.Access.WebApi.Endpoints;
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
    public static void Main(string[] args)
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

        webApplicationBuilder.Configuration
            .AddJsonFile(appsettingsPath);

        var accessOptions = webApplicationBuilder.Configuration.GetSection(AccessOptions.SectionName).Get<AccessOptions>()!;

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
            .AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "Token",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your Shuttle session token in the text input below.\r\n\r\nExample: \"Bearer {token}\""
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
            .AddSqlSubscription()
            .AddServiceBus(builder =>
            {
                webApplicationBuilder.Configuration.GetSection(ServiceBusOptions.SectionName).Bind(builder.Options);

                builder.Options.Asynchronous = true;
                builder.Options.Subscription.ConnectionStringName = "Access";

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
            })
            .AddSqlEventProcessing(builder =>
            {
                builder.Options.ConnectionStringName = "Access";
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
                foreach (var providerName in accessOptions?.OAuthProviderNames ?? Enumerable.Empty<string>())
                {
                    builder.AddOAuthOptions(providerName, webApplicationBuilder.Configuration.GetSection($"{OAuthOptions.SectionName}:{providerName}").Get<OAuthOptions>()!);
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
            .MapIdentityEndpoints(versionSet)
            .MapOAuthEndpoints(versionSet)
            .MapPermissionEndpoints(versionSet)
            .MapRoleEndpoints(versionSet)
            .MapServerEndpoints(versionSet)
            .MapSessionEndpoints(versionSet)
            .MapStatisticEndpoints(versionSet);

        app.UseSwagger()
            .UseSwaggerUI();

        app.Run();
    }
}