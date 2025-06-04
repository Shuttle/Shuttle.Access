using System;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;
using Serilog;
using Shuttle.Access.Server.v1.EventHandlers;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.DependencyInjection;
using Shuttle.Core.Mediator;
using Shuttle.Core.Pipelines;
using Shuttle.Esb;
using Shuttle.Esb.AzureStorageQueues;
using Shuttle.Esb.Sql.Subscription;
using Shuttle.Recall;
using Shuttle.Recall.Logging;
using Shuttle.Recall.Sql.EventProcessing;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Server;

internal class Program
{
    private static async Task Main(string[] args)
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

        var host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices(services =>
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile(appsettingsPath)
                    .AddEnvironmentVariables()
                    .Build();

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();

                services
                    .AddSingleton<IConfiguration>(configuration)
                    .AddSingleton(configuration.GetSection(ServerOptions.SectionName).Get<ServerOptions>() ?? new ServerOptions())
                    .FromAssembly(Assembly.Load("Shuttle.Access.Sql")).Add()
                    .AddDataAccess(builder =>
                    {
                        builder.AddConnectionString("Access", "Microsoft.Data.SqlClient");
                        builder.Options.DatabaseContextFactory.DefaultConnectionStringName = "Access";
                    })
                    .AddServiceBus(builder =>
                    {
                        configuration.GetSection(ServiceBusOptions.SectionName).Bind(builder.Options);
                    })
                    .AddAzureStorageQueues(builder =>
                    {
                        var queueOptions = configuration.GetSection($"{AzureStorageQueueOptions.SectionName}:Access").Get<AzureStorageQueueOptions>() ?? new();

                        if (string.IsNullOrWhiteSpace(queueOptions.StorageAccount))
                        {
                            queueOptions.ConnectionString = configuration.GetConnectionString("azure") ?? string.Empty;
                        }

                        builder.AddOptions("azure", queueOptions);
                    })
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
                    .AddEventStore(builder =>
                    {
                        configuration.GetSection(EventStoreOptions.SectionName).Bind(builder.Options);

                        builder.AddProjection(ProjectionNames.Identity).AddEventHandler<IdentityHandler>();
                        builder.AddProjection(ProjectionNames.Permission).AddEventHandler<PermissionHandler>();
                        builder.AddProjection(ProjectionNames.Role).AddEventHandler<RoleHandler>();
                    })
                    .AddEventStoreLogging(builder =>
                    {
                        builder.Options.AddPipelineEventType<OnPipelineException>();
                        builder.Options.AddPipelineEventType<OnAfterAcknowledgeEvent>();
                    })
                    .AddSqlSubscription(builder =>
                    {
                        builder.Options.ConnectionStringName = "Access";

                        builder.UseSqlServer();
                    })
                    .AddDataStoreAccessService()
                    .AddMediator(builder =>
                    {
                        builder.AddParticipants(Assembly.Load("Shuttle.Access.Application"));
                    })
                    .AddSingleton<IPasswordGenerator, DefaultPasswordGenerator>()
                    .AddSingleton<IHashingService, HashingService>()
                    .AddSingleton<IHostedService, ServerHostedService>()
                    .AddSingleton<KeepAliveObserver>()
                    .AddSingleton(TracerProvider.Default.GetTracer("Shuttle.Access.Server"));
            })
            .Build();

        var databaseContextFactory = host.Services.GetRequiredService<IDatabaseContextFactory>();

        var cancellationTokenSource = new CancellationTokenSource();

        Console.CancelKeyPress += delegate
        {
            cancellationTokenSource.Cancel();
        };

        if (!databaseContextFactory.IsAvailable("Access", cancellationTokenSource.Token))
        {
            throw new ApplicationException("[connection failure]");
        }

        if (cancellationTokenSource.Token.IsCancellationRequested)
        {
            return;
        }

        await host.RunAsync(cancellationTokenSource.Token);
    }
}