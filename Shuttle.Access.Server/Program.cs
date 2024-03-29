﻿using System;
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
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Data;
using Shuttle.Core.DependencyInjection;
using Shuttle.Core.Mediator;
using Shuttle.Core.Mediator.OpenTelemetry;
using Shuttle.Core.Pipelines;
using Shuttle.Esb;
using Shuttle.Esb.AzureStorageQueues;
using Shuttle.Esb.OpenTelemetry;
using Shuttle.Esb.Sql.Subscription;
using Shuttle.Recall;
using Shuttle.Recall.OpenTelemetry;
using Shuttle.Recall.Sql.Storage;
using Shuttle.Sentinel.Module;

namespace Shuttle.Access.Server
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

                    services.AddSentinelModule();

                    services.FromAssembly(Assembly.Load("Shuttle.Access.Sql")).Add();

                    services.AddDataAccess(builder =>
                    {
                        builder.AddConnectionString("Access", "Microsoft.Data.SqlClient");
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
                            .AddServiceBusInstrumentation(openTelemetryBuilder =>
                            {
                                configuration.GetSection(ServiceBusOpenTelemetryOptions.SectionName).Bind(openTelemetryBuilder.Options);
                            })
                            .AddMediatorInstrumentation(openTelemetryBuilder =>
                            {
                                configuration.GetSection(MediatorOpenTelemetryOptions.SectionName).Bind(openTelemetryBuilder.Options);
                            })
                            .AddRecallInstrumentation(openTelemetryBuilder =>
                            {
                                configuration.GetSection(RecallOpenTelemetryOptions.SectionName).Bind(openTelemetryBuilder.Options);
                            })
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

            host.Services.GetRequiredService<IPipelineFactory>().ModulesResolved += (sender, e) =>
            {
                using (databaseContextFactory.Create())
                {
                    host.Services.GetRequiredService<IMediator>().Send(new ConfigureApplication(), cancellationTokenSource.Token);
                }
            };

            host.Run();
        }
    }
}