using System;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;
using Shuttle.Core.Data;
using Shuttle.Core.DependencyInjection;
using Shuttle.Core.Mediator;
using Shuttle.Core.Mediator.OpenTelemetry;
using Shuttle.Esb;
using Shuttle.Esb.AzureStorageQueues;
using Shuttle.Esb.OpenTelemetry;
using Shuttle.Esb.Sql.Subscription;
using Shuttle.Recall;
using Shuttle.Recall.OpenTelemetry;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Server;

internal class Program
{
    private static async Task Main(string[] args)
    {
        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

                services
                    .AddSingleton<IConfiguration>(configuration)
                    .FromAssembly(Assembly.Load("Shuttle.Access.Sql")).Add()
                    .AddDataAccess(builder =>
                    {
                        builder.AddConnectionString("Access", "Microsoft.Data.SqlClient");
                        builder.Options.DatabaseContextFactory.DefaultConnectionStringName = "Access";
                    })
                    .AddServiceBus(builder =>
                    {
                        configuration.GetSection(ServiceBusOptions.SectionName).Bind(builder.Options);

                        builder.Options.Subscription.ConnectionStringName = "Access";

                        builder.Options.Asynchronous = true;

                    })
                    .AddAzureStorageQueues(builder =>
                    {
                        builder.AddOptions("azure", new AzureStorageQueueOptions
                        {
                            ConnectionString = configuration.GetConnectionString("azure")
                        });
                    })
                    .AddSqlEventStorage(builder =>
                    {
                        builder.Options.ConnectionStringName = "Access";
                    })
                    .AddEventStore()
                    .AddSqlSubscription()
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