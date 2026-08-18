using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shuttle.Access.AspNetCore;

public static class HttpRequestExtensions
{
    public const string TenantIdHeaderName = "Shuttle-Access-Tenant-Id";

    extension(HttpRequest httpRequest)
    {
        public Guid? GetTenantId(ILogger logger, Guid systemTenantId)
        {
            var tenantIdValue = httpRequest.Headers[TenantIdHeaderName].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(tenantIdValue))
            {
                LogMessage.TenantIdHeader(logger, $"Found '{TenantIdHeaderName}' header.", tenantIdValue);

                if (Guid.TryParse(tenantIdValue, out var id))
                {
                    LogMessage.TenantId(logger, "Parsed tenant id.", id);
                    return id;
                }

                LogMessage.InvalidTenantIdHeader(logger, $"Invalid GUID '{tenantIdValue}' passed as header '{TenantIdHeaderName}'.");

                return null;
            }

            LogMessage.TenantId(logger, $"No '{TenantIdHeaderName}' header found.  Using system tenant id.", systemTenantId);

            return systemTenantId;
        }
    }
}
