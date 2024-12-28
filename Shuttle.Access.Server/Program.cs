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
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.DependencyInjection;
using Shuttle.Core.Mediator;
using Shuttle.Esb;
using Shuttle.Esb.AzureStorageQueues;
using Shuttle.Esb.Sql.Subscription;
using Shuttle.Recall;
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
            .ConfigureServices(services =>
            {
                var configuration = new ConfigurationBuilder().AddJsonFile(appsettingsPath).Build();

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();

                services
                    .AddSingleton<IConfiguration>(configuration)
                    .AddLogging(builder =>
                    {
                        builder.AddSerilog();
                    })
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
                        builder.AddOptions("azure", new()
                        {
                            ConnectionString = Guard.AgainstNullOrEmptyString(configuration.GetConnectionString("azure"))
                        });
                    })
                    .AddSqlEventStorage(builder =>
                    {
                        builder.Options.ConnectionStringName = "Access";

                        builder.UseSqlServer();
                    })
                    .AddEventStore()
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
                    .AddSingleton<IHostedService, ApplicationHostedService>()
                    .AddSingleton(TracerProvider.Default.GetTracer("Shuttle.Access.Server"));

                //services.AddOpenTelemetry()
                //    .WithTracing(
                //        builder => builder
                //            .AddServiceBusInstrumentation(openTelemetryBuilder =>
                //            {
                //                configuration.GetSection(ServiceBusOpenTelemetryOptions.SectionName).Bind(openTelemetryBuilder.Options);
                //            })
                //            .AddMediatorInstrumentation(openTelemetryBuilder =>
                //            {
                //                configuration.GetSection(MediatorOpenTelemetryOptions.SectionName).Bind(openTelemetryBuilder.Options);
                //            })
                //            .AddRecallInstrumentation(openTelemetryBuilder =>
                //            {
                //                configuration.GetSection(RecallOpenTelemetryOptions.SectionName).Bind(openTelemetryBuilder.Options);
                //            })
                //            .AddSqlClientInstrumentation(options =>
                //            {
                //                options.SetDbStatementForText = true;
                //            })
                //            .AddJaegerExporter(options =>
                //            {
                //                options.AgentHost = Environment.GetEnvironmentVariable("JAEGER_AGENT_HOST");
                //            }));
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