using System.Data.Common;
using System.Reflection;
using Asp.Versioning;
using Microsoft.Data.SqlClient;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;
using Shuttle.Esb.AzureStorageQueues;
using Shuttle.Esb.Sql.Subscription;
using Shuttle.Recall;
using Shuttle.Recall.Sql.EventProcessing;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);

        var webApplicationBuilder = WebApplication.CreateBuilder(args);

        var apiVersion1 = new ApiVersion(1, 0);

        webApplicationBuilder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = apiVersion1;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        webApplicationBuilder.Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddDataAccess(builder =>
            {
                builder.AddConnectionString("Access", "Microsoft.Data.SqlClient");
                builder.Options.DatabaseContextFactory.DefaultConnectionStringName = "Access";
            })
            .AddSingleton<IHashingService, HashingService>()
            .AddSingleton<IPasswordGenerator, DefaultPasswordGenerator>()
            .AddAccess()
            .AddSqlAccess()
            .AddSqlSubscription()
            .AddServiceBus(builder =>
            {
                webApplicationBuilder.Configuration.GetSection(ServiceBusOptions.SectionName).Bind(builder.Options);

                builder.Options.Subscription.ConnectionStringName = "Access";

                builder.AddSubscription<IdentityRoleSet>();
                builder.AddSubscription<RolePermissionSet>();
                builder.AddSubscription<PermissionStatusSet>();
            })
            .AddAzureStorageQueues(builder =>
            {
                builder.AddOptions("azure", new AzureStorageQueueOptions
                {
                    ConnectionString = webApplicationBuilder.Configuration.GetConnectionString("azure")
                });
            })
            .AddEventStore()
            .AddSqlEventStorage(builder =>
            {
                builder.Options.ConnectionStringName = "Access";
            })
            .AddSqlEventProcessing(builder =>
            {
                builder.Options.ConnectionStringName = "Access";
            })
            .AddMediator(builder =>
            {
                builder.AddParticipants(Assembly.Load("Shuttle.Access.Application"));
            })
            .AddAccessAuthorization()
            .AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            }); ;

        var app = webApplicationBuilder.Build();

        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(apiVersion1)
            .ReportApiVersions()
            .Build();

        app.UseCors("AllowAll");
        app.UseAccessAuthorization();
            
        app.MapIdentityEndpoints(versionSet);
        app.MapPermissionEndpoints(versionSet);
        app.MapRoleEndpoints(versionSet);
        app.MapServerEndpoints(versionSet);
        app.MapSessionEndpoints(versionSet);
        app.MapStatisticEndpoints(versionSet);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.Run();
    }
}