using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shuttle.Access.AspNetCore;

public static class HttpRequestExtensions
{
    extension(HttpRequest httpRequest)
    {
        public Guid? GetTenantId(ILogger logger, Guid systemTenantId)
        {
            var tenantIdValue = httpRequest.Headers["Shuttle-Access-Tenant-Id"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(tenantIdValue))
            {
                LogMessage.TenantIdHeader(logger, "Found 'Shuttle-Access-Tenant-Id' header.", tenantIdValue);

                if (Guid.TryParse(tenantIdValue, out var id))
                {
                    LogMessage.TenantId(logger, "Parsed tenant id.", id);
                    return id;
                }

                LogMessage.InvalidTenantIdHeader(logger, $"Invalid GUID '{tenantIdValue}' passed as header 'Shuttle-Access-Tenant-Id'.");

                return null;
            }

            LogMessage.TenantId(logger, "No 'Shuttle-Access-Tenant-Id' header found.  Using system tenant id.", systemTenantId);

            return systemTenantId;
        }
    }
}