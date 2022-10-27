using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Mvc.DataStore;
using Shuttle.Access.Sql;
using Shuttle.Core.Data;
using Shuttle.Core.Data.Http;
using Shuttle.Core.Mediator;
using Shuttle.Core.Reflection;
using Shuttle.Esb;
using Shuttle.Esb.AzureStorageQueues;
using Shuttle.Esb.OpenTelemetry;
using Shuttle.Esb.Sql.Subscription;
using Shuttle.Recall;
using Shuttle.Recall.Sql.EventProcessing;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.WebApi
{
    public class Startup
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private void OnShutdown()
        {
            _cancellationTokenSource?.Cancel();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDatabaseContextCache, ContextDatabaseContextCache>();

            services.AddDataAccess(builder =>
            {
                builder.AddConnectionString("Access", "System.Data.SqlClient");
                builder.Options.DatabaseContextFactory.DefaultConnectionStringName = "Access";
            });

            services.AddAccess();
            services.AddSqlAccess();

            services.AddSingleton<ISessionQueryFactory, SessionQueryFactory>();
            services.AddSingleton<ISessionQuery, SessionQuery>();
            services.AddSingleton<ISessionRepository, SessionRepository>();

            services.AddSingleton<IAccessService, DataStoreAccessService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IHashingService, HashingService>();
            services.AddSingleton<IPasswordGenerator, DefaultPasswordGenerator>();

            services.AddSqlSubscription();

            services.AddServiceBus(builder =>
            {
                Configuration.GetSection(ServiceBusOptions.SectionName).Bind(builder.Options);

                builder.Options.Subscription.ConnectionStringName = "Access";

                builder.AddSubscription<IdentityRoleSet>();
                builder.AddSubscription<RolePermissionSet>();
                builder.AddSubscription<PermissionStatusSet>();
            });

            services.AddAzureStorageQueues(builder =>
            {
                builder.AddOptions("azure", new AzureStorageQueueOptions
                {
                    ConnectionString = Configuration.GetConnectionString("azure")
                });
            });

            services.AddEventStore();
            services.AddSqlEventStorage();

            services.TryAddSingleton<Recall.Sql.EventProcessing.IScriptProvider, Recall.Sql.EventProcessing.ScriptProvider>();
            services.AddSingleton<IProjectionQueryFactory, ProjectionQueryFactory>();
            services.AddSingleton<IProjectionRepository, ProjectionRepository>();

            services.AddMediator(builder =>
            {
                builder.AddParticipants(Assembly.Load("Shuttle.Access.Application"));
            });

            services.AddSingleton(TracerProvider.Default.GetTracer("Shuttle.Access.WebApi"));

            services.AddOpenTelemetryTracing(
                builder => builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Shuttle.Access.WebApi"))
                    .AddServiceBusInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                    })
                    .AddJaegerExporter());

            services.AddServiceBusInstrumentation(builder =>
            {
                Configuration.GetSection(OpenTelemetryOptions.SectionName).Bind(builder.Options);
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Shuttle.Access.WebApi", Version = "v1" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "access-session-token",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Shuttle.Access token security",
                    Scheme = "Bearer"
                });

                var key = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    In = ParameterLocation.Header
                };

                var requirement = new OpenApiSecurityRequirement
                {
                    { key, new List<string>() }
                };

                options.AddSecurityRequirement(requirement);
            });

            services.AddControllers();
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new HeaderApiVersionReader("api-version");
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IHostApplicationLifetime applicationLifetime, ILogger<Startup> logger)
        {
            var applicationPartManager = app.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            var controllerFeature = new ControllerFeature();

            applicationPartManager.PopulateFeature(controllerFeature);

            var databaseContextFactory = app.ApplicationServices.GetRequiredService<IDatabaseContextFactory>();

            if (!databaseContextFactory.IsAvailable("Access", _cancellationTokenSource.Token))
            {
                throw new ApplicationException("[connection failure]");
            }

            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler(error =>
            {
                error.Run(context =>
                {
                    var feature = context.Features.Get<IExceptionHandlerFeature>();

                    if (feature != null)
                    {
                        logger.LogError(feature.Error.AllMessages());
                    }

                    return Task.CompletedTask;
                });
            });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseCors(
                options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
            );

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shuttle.Access.WebApi.v1");
            });
        }
    }
}