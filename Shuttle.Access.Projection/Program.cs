using System;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shuttle.Access.Projection.v1;
using Shuttle.Core.Data;
using Shuttle.Core.DependencyInjection;
using Shuttle.Recall;
using Shuttle.Recall.Sql.EventProcessing;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Projection
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

                    services.AddSingleton<IConfiguration>(configuration);

                    services.FromAssembly(Assembly.Load("Shuttle.Access.Sql")).Add();

                    services.AddDataAccess(builder =>
                    {
                        builder.AddConnectionString("Access", "Microsoft.Data.SqlClient");
                        builder.Options.DatabaseContextFactory.DefaultConnectionStringName = "Access";
                    });

                    services.AddEventStore(builder =>
                    {
                        builder.AddEventHandler<IdentityHandler>(ProjectionNames.Identity);
                        builder.AddEventHandler<PermissionHandler>(ProjectionNames.Permission);
                        builder.AddEventHandler<RoleHandler>(ProjectionNames.Role);
                    });

                    services.AddSqlEventStorage();
                    services.AddSqlEventProcessing(builder =>
                    {
                        builder.Options.EventProjectionConnectionStringName = "Access";
                        builder.Options.EventStoreConnectionStringName = "Access";
                    });

                    services.AddSingleton<IHashingService, HashingService>();

                    services.AddSingleton(TracerProvider.Default.GetTracer("Shuttle.Access.Projection"));

                    services.AddOpenTelemetryTracing(
                        builder => builder
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Shuttle.Access.Projection"))
                            .AddSqlClientInstrumentation(options =>
                            {
                                options.SetDbStatementForText = true;
                            })
                            .AddJaegerExporter(options =>
                            {
                                options.AgentHost = Environment.GetEnvironmentVariable("JAEGER_AGENT_HOST");
                            }));
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

            host.Run();
        }
    }
}