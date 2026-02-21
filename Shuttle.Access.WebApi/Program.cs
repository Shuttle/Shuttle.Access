using System.Data.Common;
using System.Reflection;
using Asp.Versioning;
using Microsoft.Data.SqlClient;
using Scalar.AspNetCore;
using Serilog;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;
using Shuttle.Hopper.AzureStorageQueues;
using Shuttle.Hopper.SqlServer.Subscription;
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

        webApplicationBuilder.Configuration
            .AddJsonFile(appsettingsPath)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables();

        var accessConnectionString = webApplicationBuilder.Configuration.GetConnectionString("Access") ?? "Missing connection string 'Access'.";

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

        webApplicationBuilder.Services
            .Configure<ApiOptions>(webApplicationBuilder.Configuration.GetSection(ApiOptions.SectionName))
            .AddSingleton<IContextSessionService, NullContextSessionService>()
            .AddSingleton<IHashingService, HashingService>()
            .AddSingleton<IPasswordGenerator, DefaultPasswordGenerator>()
            .AddScoped<ISessionContext, SessionContext>()
            .AddAccess(accessBuilder =>
            {
                webApplicationBuilder.Configuration.GetSection(AccessOptions.SectionName).Bind(accessBuilder.Options);

                accessBuilder
                    .UseSqlServer(builder =>
                    {
                        builder.Options.ConnectionString = accessConnectionString;
                    });
            })
            .AddHopper(hopperBuilder =>
            {
                webApplicationBuilder.Configuration.GetSection(HopperOptions.SectionName).Bind(hopperBuilder.Options);

                hopperBuilder
                    .UseAzureStorageQueues(builder =>
                    {
                        var queueOptions = webApplicationBuilder.Configuration.GetSection($"{AzureStorageQueueOptions.SectionName}:Access").Get<AzureStorageQueueOptions>() ?? new();

                        if (string.IsNullOrWhiteSpace(queueOptions.StorageAccount))
                        {
                            queueOptions.ConnectionString = webApplicationBuilder.Configuration.GetConnectionString("azure") ?? string.Empty;
                        }

                        builder.AddOptions("azure", queueOptions);
                    })
                    .UseSqlServerSubscription(builder =>
                    {
                        builder.Options.ConnectionString = accessConnectionString;
                        builder.Options.Schema = "access";
                    })
                    .AddSubscription<IdentityRoleSet>()
                    .AddSubscription<RolePermissionSet>()
                    .AddSubscription<PermissionStatusSet>();
            })
            .AddRecall(recallBuilder =>
            {
                recallBuilder
                    .UseSqlServerEventStorage(builder =>
                    {
                        builder.Options.ConnectionString = accessConnectionString;
                        builder.Options.Schema = "access";
                    })
                    .UseSqlServerEventProcessing(builder =>
                    {
                        builder.Options.ConnectionString = accessConnectionString;
                        builder.Options.Schema = "access";
                    });

                recallBuilder.SuppressEventProcessorHostedService();
                recallBuilder.SuppressPrimitiveEventSequencerHostedService();
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
            .AddOAuth(builder =>
            {
                webApplicationBuilder.Configuration.GetSection(OAuthOptions.SectionName).Bind(builder.Options);

                builder.UseInMemoryOAuthGrantRepository();
            })
            .AddHostedService<LoggingHostedService>();

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

        app.Use(async (context, next) =>
        {
            var sessionContext = context.RequestServices.GetRequiredService<ISessionContext>();

            var identityId = context.FindIdentityId();
            var tenantId = context.FindTenantId();

            if (identityId != null)
            {
                var sessionRepository = context.RequestServices.GetRequiredService<ISessionRepository>();

                sessionContext.Session = await sessionRepository.FindAsync(new SessionSpecification()
                    .WithTenantId(tenantId)
                    .WithIdentityId(identityId.Value));
            }
            else
            {
                sessionContext.Session = null;
            }

            await next();
        });

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