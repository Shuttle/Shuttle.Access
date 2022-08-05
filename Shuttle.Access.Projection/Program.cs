using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                    });

                    services.AddEventStore(builder =>
                    {
                        builder.AddEventHandler<IdentityHandler>(ProjectionNames.Identity);
                        builder.AddEventHandler<PermissionHandler>(ProjectionNames.Permission);
                        builder.AddEventHandler<RoleHandler>(ProjectionNames.Role);
                    });

                    services.AddSqlEventStorage();
                    services.AddSqlEventProcessing();

                    services.AddSingleton<IHashingService, HashingService>();
                })
                .Build();

            var databaseContextFactory = host.Services.GetRequiredService<IDatabaseContextFactory>().ConfigureWith("Access");

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