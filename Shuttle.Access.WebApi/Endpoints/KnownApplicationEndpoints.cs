using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.WebApi;

public static class KnownApplicationEndpoints
{
    public static WebApplication MapApplicationEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapGet("/v{version:apiVersion}/applications/{applicationName}", (IOptions<AccessOptions> accessOptions, string applicationName) =>
            {
                Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
                Guard.AgainstNullOrEmptyString(applicationName);

                var knownApplicationOptions = accessOptions.Value.KnownApplications.FirstOrDefault(item => item.Name.Equals(applicationName, StringComparison.InvariantCultureIgnoreCase));

                if (knownApplicationOptions == null)
                {
                    return Results.NotFound();
                }

                var result = new KnownApplication
                {
                    Name = knownApplicationOptions.Name,
                    Title = knownApplicationOptions.Title,
                    Description = knownApplicationOptions.Description
                };

                var path = Path.Combine(accessOptions.Value.SvgFolder, "KnownApplication", $"{knownApplicationOptions.Name}.svg");

                if (File.Exists(path))
                {
                    result.Svg = File.ReadAllText(path);
                }

                return Results.Ok(result);
            })
            .WithTags("Applications")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }
}