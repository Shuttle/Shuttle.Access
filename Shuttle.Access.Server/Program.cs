using System.Data.Common;
using System.Reflection;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Shuttle.Access.SqlServer;
using Shuttle.Access.Server.v1.EventHandlers;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;
using Shuttle.Hopper.AzureStorageQueues;
using Shuttle.Hopper.SqlServer.Subscription;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.EventProcessing;
using Shuttle.Recall.SqlServer.Storage;

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
                    .AddSingleton(configuration.GetSection(ServerOptions.SectionName).Get<ServerOptions>() ?? new ServerOptions())
                    .AddAccess(accessBuilder =>
                    {
                        accessBuilder.UseSqlServer(builder =>
                        {
                            builder.Options.ConnectionString = accessConnectionString;
                        });
                    })
                    .AddHopper(hopperBuilder =>
                    {
                        configuration.GetSection(HopperOptions.SectionName).Bind(hopperBuilder.Options);

                        hopperBuilder
                            .UseAzureStorageQueues(builder =>
                            {
                                var queueOptions = configuration.GetSection($"{AzureStorageQueueOptions.SectionName}:Access").Get<AzureStorageQueueOptions>() ?? new();

                                if (string.IsNullOrWhiteSpace(queueOptions.StorageAccount))
                                {
                                    queueOptions.ConnectionString = configuration.GetConnectionString("azure") ?? throw new ApplicationException("Missing connection string 'azure'.");
                                }

                                builder.AddOptions("azure", queueOptions);
                            })
                            .UseSqlServerSubscription(builder =>
                            {
                                builder.Options.ConnectionString = accessConnectionString;
                                builder.Options.Schema = "access";
                            });
                    })
                    .AddRecall(recallBuilder =>
                    {
                        configuration.GetSection(RecallOptions.SectionName).Bind(recallBuilder.Options);

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

                        recallBuilder.AddProjection(ProjectionNames.Identity).AddEventHandler<IdentityHandler>();
                        recallBuilder.AddProjection(ProjectionNames.Permission).AddEventHandler<PermissionHandler>();
                        recallBuilder.AddProjection(ProjectionNames.Role).AddEventHandler<RoleHandler>();
                    })
                    .AddMediator(builder =>
                    {
                        builder.AddParticipants(Assembly.Load("Shuttle.Access.Application"));
                    })
                    .AddSingleton<IPasswordGenerator, DefaultPasswordGenerator>()
                    .AddSingleton<IHashingService, HashingService>()
                    .AddSingleton<IHostedService, ServerHostedService>()
                    .AddScoped<KeepAliveObserver>();
            })
            .Build()
            .RunAsync();
    }
}