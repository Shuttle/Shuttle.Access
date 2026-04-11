using System.Data.Common;
using Asp.Versioning;
using Microsoft.Data.SqlClient;
using Scalar.AspNetCore;
using Serilog;
using Shuttle.Access.Application;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.SqlServer;
using Shuttle.Hopper;
using Shuttle.Hopper.AzureStorageQueues;
using Shuttle.Hopper.SqlServer.Subscription;
using Shuttle.Mediator;
using Shuttle.OAuth;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.EventProcessing;
using Shuttle.Recall.SqlServer.Storage;

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

        var configuration = webApplicationBuilder.Configuration;
        var services = webApplicationBuilder.Services;

        configuration
            .AddJsonFile(appsettingsPath)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables();

        var accessConnectionString = configuration.GetConnectionString("Access") ?? "Missing connection string 'Access'.";

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        var apiVersion1 = new ApiVersion(1, 0);

        services
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

        services
            .AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            })
            .AddEndpointsApiExplorer()
            .AddOpenApi(options =>
            {
                options.AddSchemaTransformer((schema, _, _) =>
                {
                    schema.Title = schema.Title?.Replace("+", "_");
                    return Task.CompletedTask;
                });
            });

        services
            .Configure<ApiOptions>(configuration.GetSection(ApiOptions.SectionName))
            .AddSingleton<IContextSessionService, NullContextSessionService>()
            .AddSingleton<IHashingService, HashingService>()
            .AddSingleton<IPasswordGenerator, DefaultPasswordGenerator>()
            .AddAccess(options =>
            {
                configuration.GetSection(AccessOptions.SectionName).Bind(options);
            })
            .UseSqlServer(builder =>
            {
                builder.Options.ConnectionString = accessConnectionString;
            })
            .Services
            .AddHopper(options =>
            {
                configuration.GetSection(HopperOptions.SectionName).Bind(options);
            })
            .UseAzureStorageQueues(builder =>
            {
                builder.Configure("azure", options =>
                {
                    configuration.GetSection($"{AzureStorageQueueOptions.SectionName}:Access").Bind(options);

                    if (string.IsNullOrWhiteSpace(options.StorageAccount))
                    {
                        options.ConnectionString = configuration.GetConnectionString("azure") ?? string.Empty;
                    }
                });
            })
            .AddMessageHandlersFrom(typeof(Program).Assembly)
            .UseSqlServerSubscription(sqlServerSubscriptionOptions =>
            {
                sqlServerSubscriptionOptions.ConnectionString = accessConnectionString;
                sqlServerSubscriptionOptions.Schema = "access";
            })
            .AddSubscription<IdentityRoleAdded>()
            .AddSubscription<IdentityRoleRemoved>()
            .AddSubscription<RolePermissionAdded>()
            .AddSubscription<RolePermissionRemoved>()
            .AddSubscription<PermissionStatusSet>()
            .Services
            .AddRecall()
            .UseSqlServerEventStorage(options =>
            {
                options.ConnectionString = accessConnectionString;
                options.Schema = "access";
            })
            .UseSqlServerEventProcessing()
            .Services
            .AddMediator()
            .AddParticipantsFrom(typeof(ConfigureApplication).Assembly)
            .Services
            .AddAccessAuthorization(options =>
            {
                configuration.GetSection(AccessAuthorizationOptions.SectionName).Bind(options);

                options.PassThrough = false;
            })
            .Services
            .AddOAuth(builder =>
            {
                configuration.GetSection(OAuthOptions.SectionName).Bind(builder.Options);

                builder.UseInMemoryOAuthGrantRepository();
            });

        var app = webApplicationBuilder.Build();

        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(apiVersion1)
            .ReportApiVersions()
            .Build();

        app.UseCors();

        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("Shuttle Access API")
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        app.UseAccessAuthorization();

        app
            .MapIdentityEndpoints(versionSet)
            .MapOAuthEndpoints(versionSet)
            .MapPermissionEndpoints(versionSet)
            .MapRoleEndpoints(versionSet)
            .MapServerEndpoints(versionSet)
            .MapSessionEndpoints(versionSet)
            .MapStatisticEndpoints(versionSet)
            .MapTenantEndpoints(versionSet);

        await app.RunAsync();
    }
}