using System.Data.Common;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Shuttle.Access.Application;
using Shuttle.Access.Server.v1.EventHandlers;
using Shuttle.Access.SqlServer;
using Shuttle.Hopper;
using Shuttle.Hopper.AzureStorageQueues;
using Shuttle.Hopper.SqlServer.Subscription;
using Shuttle.Mediator;
using Shuttle.Pipelines;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.EventProcessing;
using Shuttle.Recall.SqlServer.Storage;
using Shuttle.Reflection;

namespace Shuttle.Access.Server;

internal class Program
{
    private static async Task Main()
    {
        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

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

        await Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices(services =>
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile(appsettingsPath)
                    .AddUserSecrets<Program>()
                    .AddEnvironmentVariables()
                    .Build();

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();

                var accessConnectionString = configuration.GetConnectionString("Access") ?? throw new ApplicationException("Missing connection string 'Access'.");

                services
                    .AddSingleton<IConfiguration>(configuration)
                    .AddSingleton<IKeepAliveContext, KeepAliveContext>()
                    .AddSingleton(configuration.GetSection(ServerOptions.SectionName).Get<ServerOptions>() ?? new ServerOptions())
                    .AddAccess()
                    .UseSqlServer(options =>
                    {
                        options.ConnectionString = accessConnectionString;
                    })
                    .Services
                    .AddPipelines(options =>
                    {
                        options.PipelineFailed += (eventArgs, _) =>
                        {
                            Log.Error(eventArgs.Pipeline.Exception?.AllMessages() ?? string.Empty);
                            return Task.CompletedTask;
                        };

                        options.PipelineRecursiveException += (eventArgs, _) =>
                        {
                            Log.Error(eventArgs.Pipeline.Exception?.AllMessages() ?? string.Empty);
                            return Task.CompletedTask;
                        };
                    })
                    .Services
                    .AddHopper(options => configuration.GetSection(HopperOptions.SectionName).Bind(options))
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
                    .UseSqlServerSubscription(options =>
                    {
                        options.ConnectionString = accessConnectionString;
                        options.Schema = "access";
                    })
                    .AddMessageHandlersFrom(typeof(Program).Assembly)
                    .Services
                    .AddRecall(options =>
                    {
                        configuration.GetSection(RecallOptions.SectionName).Bind(options);
                    })
                    .UseSqlServerEventStorage(options =>
                    {
                        configuration.GetSection(SqlServerStorageOptions.SectionName).Bind(options);

                        options.ConnectionString = accessConnectionString;
                        options.Schema = "access";
                    })
                    .RegisterPrimitiveEventSequencing()
                    .UseSqlServerEventProcessing()
                    .AddProjection<IdentityHandler>(ProjectionNames.Identity)
                    .AddProjection<PermissionHandler>(ProjectionNames.Permission)
                    .AddProjection<RoleHandler>(ProjectionNames.Role)
                    .AddProjection<TenantHandler>(ProjectionNames.Tenant)
                    .Services
                    .AddMediator()
                    .AddParticipantsFrom(typeof(ConfigureApplication).Assembly)
                    .Services
                    .AddSingleton<IPasswordGenerator, DefaultPasswordGenerator>()
                    .AddSingleton<IHashingService, HashingService>()
                    .AddSingleton<IHostedService, ServerHostedService>()
                    .AddScoped<KeepAliveObserver>();
            })
            .Build()
            .RunAsync();
    }
}