using System.Reflection;
using Asp.Versioning;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var webApplicationBuilder = WebApplication.CreateBuilder(args);

        var apiVersion1 = new ApiVersion(1, 0);

        webApplicationBuilder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = apiVersion1;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });

        webApplicationBuilder.Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddAccessAuthorization();

        var app = webApplicationBuilder.Build();

        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(apiVersion1)
            .ReportApiVersions()
            .Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAccessAuthorization();

        app.MapGet("/server/configuration", (HttpContext _) =>
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);

            return new { Version = $"{version.Major}.{version.Minor}.{version.Build}" };
        });

        app.MapPost("/v{version:apiVersion}/sessions/delegated", async (HttpContext httpContext, IDatabaseContextFactory databaseContextFactory, ISessionService sessionService, RegisterDelegatedSession message) =>
        {
            if (string.IsNullOrEmpty(message.IdentityName))
            {
                return Results.BadRequest();
            }

            var sessionTokenResult = httpContext.GetAccessSessionToken();

            if (!sessionTokenResult.Ok)
            {
                return Results.Unauthorized();
            }

            RegisterSessionResult registerSessionResult;

            await using (databaseContextFactory.Create())
            {
                registerSessionResult = await sessionService.RegisterAsync(message.IdentityName, sessionTokenResult.SessionToken);
            }

            if (!registerSessionResult.Ok)
            {
                return Results.BadRequest();
            }

            return Results.Ok(new SessionRegistered
            {
                IdentityName = registerSessionResult.IdentityName,
                Token = registerSessionResult.Token,
                TokenExpiryDate = registerSessionResult.TokenExpiryDate,
                Permissions = registerSessionResult.Permissions.ToList()
            });
        })
            .RequiresSession()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapDelete("/v{version:apiVersion}/sessions", async (HttpContext httpContext, IDatabaseContextFactory databaseContextFactory, ISessionService sessionService) =>
        {
            var sessionTokenResult = httpContext.GetAccessSessionToken();

            if (!sessionTokenResult.Ok)
            {
                return Results.BadRequest();
            }

            await using (databaseContextFactory.Create())
            {
                return await sessionService.RemoveAsync(sessionTokenResult.SessionToken)
                    ? Results.Ok()
                    : Results.Problem();
            }
        })
            .RequiresPermission(Permissions.View.Sessions)
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/sessions/{token}", async (Guid token, IDatabaseContextFactory databaseContextFactory, ISessionQuery sessionQuery) =>
        {
            await using (databaseContextFactory.Create())
            {
                var session = await sessionQuery.GetAsync(token);

                return session == null
                    ? Results.BadRequest()
                    : Results.Ok(session);
            }
        }).RequireAuthorization(Permissions.View.Sessions);

        app.MapGet("/sessions/{token}/permissions", async (Guid token, IDatabaseContextFactory databaseContextFactory, ISessionRepository sessionRepository) =>
        {
            await using (databaseContextFactory.Create())
            {
                var session = await sessionRepository.FindAsync(token);

                return session == null
                    ? Results.BadRequest()
                    : Results.Ok(session.Permissions);
            }
        }).RequireAuthorization(Permissions.View.Sessions);

        app.Run();
    }
}