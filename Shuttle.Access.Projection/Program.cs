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
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using Shuttle.Access.Projection.v1;
using Shuttle.Core.Data;
using Shuttle.Core.DependencyInjection;
using Shuttle.Core.Pipelines;
using Shuttle.Recall;
using Shuttle.Recall.Logging;
using Shuttle.Recall.Sql.EventProcessing;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Projection;

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

                services
                    .AddSingleton<IConfiguration>(configuration)
                    .AddLogging(configure =>
                    {
                        configure.AddConsole();
                    })
                    .FromAssembly(Assembly.Load("Shuttle.Access.Sql")).Add()
                    .AddDataAccess(builder =>
                    {
                        builder.AddConnectionString("Access", "Microsoft.Data.SqlClient");
                        builder.Options.DatabaseContextFactory.DefaultConnectionStringName = "Access";
                    })
                    .AddSqlEventStorage(builder =>
                    {
                        builder.Options.ConnectionStringName = "Access";
                    })
                    .AddSqlEventProcessing(builder =>
                    {
                        builder.Options.ConnectionStringName = "Access";
                    })
                    .AddEventStore(builder =>
                    {
                        configuration.GetSection(EventStoreOptions.SectionName).Bind(builder.Options);
                        builder.Options.Asynchronous = true;

                        builder.AddEventHandler<IdentityHandler>(ProjectionNames.Identity);
                        builder.AddEventHandler<PermissionHandler>(ProjectionNames.Permission);
                        builder.AddEventHandler<RoleHandler>(ProjectionNames.Role);
                    })
                    .AddRecallLogging(builder =>
                    {
                        builder.Options.AddPipelineEventType<OnPipelineException>();
                    })
                    .AddSingleton<IHashingService, HashingService>()
                    .AddSingleton(TracerProvider.Default.GetTracer("Shuttle.Access.Projection"));

                //services.AddOpenTelemetry()
                //    .WithTracing(builder =>
                //    {
                //        builder.AddRecallInstrumentation(openTelemetryBuilder =>
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
                //            });
                //    });
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