using System.Reflection;
using Asp.Versioning;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.WebApi;

public static class ServerEndpoints
{
    public static void MapServerEndpoints(this WebApplication app)
    {
        var apiVersion1 = new ApiVersion(1, 0);
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(apiVersion1)
            .ReportApiVersions()
            .Build();

        app.MapGet("/v1/server/configuration", (HttpContext _) =>
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);

                return new ServerConfiguration { Version = $"{version.Major}.{version.Minor}.{version.Build}" };
            })
            .WithTags("Server")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);
    }
}