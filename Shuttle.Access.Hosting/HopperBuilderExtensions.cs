using Microsoft.Extensions.Configuration;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Hopper.AzureStorageQueues;
using Shuttle.Hopper.RabbitMQ;

namespace Shuttle.Access.Hosting;

public static class HopperBuilderExtensions
{
    extension(HopperBuilder hopperBuilder)
    {
        /// <summary>
        ///     Registers each transport whose configuration section is present. The transport used for a given queue is
        ///     selected by the scheme of its URI (`azuresq://`, `rabbitmq://`), and the URI host selects the named options
        ///     instance (the child key of the transport section).
        /// </summary>
        public HopperBuilder UseConfiguredTransports(IConfiguration configuration)
        {
            Guard.AgainstNull(configuration);

            var registered = false;

            hopperBuilder
                .TryUseAzureStorageQueues(configuration, ref registered)
                .TryUseRabbitMQ(configuration, ref registered);

            if (!registered)
            {
                throw new ApplicationException($"No transport has been configured.  At least one of the following configuration sections is required, with a child section per transport name (e.g. `{AzureStorageQueueOptions.SectionName}:azure`): `{AzureStorageQueueOptions.SectionName}`, `{RabbitMQOptions.SectionName}`.");
            }

            return hopperBuilder;
        }

        private HopperBuilder TryUseAzureStorageQueues(IConfiguration configuration, ref bool registered)
        {
            var transports = configuration.GetSection(AzureStorageQueueOptions.SectionName).GetChildren().ToList();

            if (transports.Count == 0)
            {
                return hopperBuilder;
            }

            registered = true;

            return hopperBuilder.UseAzureStorageQueues(builder =>
            {
                foreach (var transport in transports)
                {
                    builder.Configure(transport.Key, options =>
                    {
                        transport.Bind(options);

                        if (string.IsNullOrWhiteSpace(options.StorageAccount) && string.IsNullOrWhiteSpace(options.ConnectionString))
                        {
                            options.ConnectionString = configuration.GetConnectionString(transport.Key) ?? throw new ApplicationException($"Azure Storage Queues transport '{transport.Key}' requires either a `StorageAccount`, a `ConnectionString`, or a connection string named '{transport.Key}'.");
                        }
                    });
                }
            });
        }

        private HopperBuilder TryUseRabbitMQ(IConfiguration configuration, ref bool registered)
        {
            var transports = configuration.GetSection(RabbitMQOptions.SectionName).GetChildren().ToList();

            if (transports.Count == 0)
            {
                return hopperBuilder;
            }

            registered = true;

            return hopperBuilder.UseRabbitMQ(builder =>
            {
                foreach (var transport in transports)
                {
                    builder.Configure(transport.Key, options => transport.Bind(options));
                }
            });
        }
    }
}
