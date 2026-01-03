using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.WebApi;

public static class ServerEndpoints
{
    public static WebApplication MapServerEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapGet("/v{version:apiVersion}/server/configuration", (IOptions<AccessOptions> accessOptions) =>
            {
                var options = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);

                var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);

                return new ServerConfiguration
                {
                    Version = $"{version.Major}.{version.Minor}.{version.Build}",
                    AllowPasswordAuthentication = options.AllowPasswordAuthentication
                };
            })
            .WithTags("Server")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }
}