using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Data;
using Shuttle.Core.DependencyInjection;
using Shuttle.Core.Mediator;
using Shuttle.Esb;
using Shuttle.Esb.AzureStorageQueues;
using Shuttle.Esb.OpenTelemetry;
using Shuttle.Esb.Sql.Subscription;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

                    services.AddSingleton<IConfiguration>(configuration);

                    services.FromAssembly(Assembly.Load("Shuttle.Access.Sql")).Add();

                    services.AddDataAccess(builder =>
                    {
                        builder.AddConnectionString("Access", "System.Data.SqlClient");
                        builder.Options.DatabaseContextFactory.DefaultConnectionStringName = "Access";
                    });

                    services.AddServiceBus(builder =>
                    {
                        configuration.GetSection(ServiceBusOptions.SectionName).Bind(builder.Options);

                        builder.Options.Subscription.ConnectionStringName = "Access";
                    });

                    services.AddAzureStorageQueues(builder =>
                    {
                        builder.AddOptions("azure", new AzureStorageQueueOptions
                        {
                            ConnectionString = configuration.GetConnectionString("azure")
                        });
                    });

                    services.AddEventStore();
                    services.AddSqlEventStorage();
                    services.AddSqlSubscription();

                    services.AddMediator(builder =>
                    {
                        builder.AddParticipants(Assembly.Load("Shuttle.Access.Application"));
                    });

                    services.AddSingleton<IPasswordGenerator, DefaultPasswordGenerator>();
                    services.AddSingleton<IHashingService, HashingService>();

                    services.AddSingleton(TracerProvider.Default.GetTracer("Shuttle.Access.Server"));

                    services.AddOpenTelemetryTracing(
                        builder => builder
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Shuttle.Access.Server"))
                            .AddServiceBusInstrumentation()
                            .AddSqlClientInstrumentation(options =>
                            {
                                options.SetDbStatementForText = true;
                            })
                            .AddJaegerExporter());

                    services.AddServiceBusInstrumentation(builder =>
                    {
                        configuration.GetSection(OpenTelemetryOptions.SectionName).Bind(builder.Options);
                    });
                })
                .Build();

            var databaseContextFactory = host.Services.GetRequiredService<IDatabaseContextFactory>();

            var cancellationTokenSource = new CancellationTokenSource();

            Console.CancelKeyPress += delegate {
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

            using (databaseContextFactory.Create())
            {
                host.Services.GetRequiredService<IMediator>().Send(new ConfigureApplication(), cancellationTokenSource.Token);
            }

            host.Run();
        }
    }
}