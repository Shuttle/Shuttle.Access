using System.Configuration;
using System.Data.Common;
using System.Reflection;
using Asp.Versioning;
using Microsoft.Data.SqlClient;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
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

        foreach (var providerName in (accessOptions?.OAuthProviderNames ?? Enumerable.Empty<string>()))
        {
            webApplicationBuilder.Services.Configure<OAuthOptions>(providerName, options =>
            {
                var key = $"{OAuthOptions.SectionName}:{providerName}";
                var oauthOptions = webApplicationBuilder.Configuration.GetSection(key).Get<OAuthOptions>();

                if (oauthOptions == null)
                {
                    throw new ApplicationException($"OAuth options configuration section '{key}' not found.");
                }

                options.ClientId = oauthOptions.ClientId;
                options.ClientSecret = oauthOptions.ClientSecret;
                options.TokenUrl = oauthOptions.TokenUrl;
                options.TokenContentType = oauthOptions.TokenContentType;
                options.DataUrl = oauthOptions.DataUrl;
                options.DataAuthorization = oauthOptions.DataAuthorization;
                options.DataAccept = oauthOptions.DataAccept;
                options.CodeChallengeMethod = oauthOptions.CodeChallengeMethod;
                options.Scope = oauthOptions.Scope;
                options.EMailPropertyName = oauthOptions.EMailPropertyName;
            });
        }

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
            .AddAccess(builder =>
            {
                webApplicationBuilder.Configuration.GetSection(AccessOptions.SectionName).Bind(builder.Options);
            })
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
                builder.AddOptions("azure", new()
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
            .AddAccessAuthorization()
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
                builder.AddOAuthOptions("GitHub", webApplicationBuilder.Configuration.GetSection($"{OAuthOptions.SectionName}:GitHub").Get<OAuthOptions>()!);
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

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.Run();
    }
}