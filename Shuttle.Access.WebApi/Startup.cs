using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.Windsor;
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
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Mvc.DataStore;
using Shuttle.Access.Sql;
using Shuttle.Core.Castle;
using Shuttle.Core.Container;
using Shuttle.Core.Data;
using Shuttle.Core.Data.Http;
using Shuttle.Core.Logging;
using Shuttle.Core.Mediator;
using Shuttle.Core.Reflection;
using Shuttle.Esb;
using Shuttle.Esb.AzureMQ;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.WebApi
{
    public class Startup
    {
        private IServiceBus _bus;
        
        private readonly ILog _log;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _log = Log.For(this);
        }

        public IConfiguration Configuration { get; }

        private void OnShutdown()
        {
            _bus?.Dispose();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IWindsorContainer>(new WindsorContainer());
            services.AddSingleton<IControllerActivator, ControllerActivator>();

            services.AddSingleton(AccessSection.Configuration());
            services.AddSingleton<IConnectionConfigurationProvider, ConnectionConfigurationProvider>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IDatabaseContextCache, ContextDatabaseContextCache>();
            services.AddSingleton<IDatabaseContextFactory, DatabaseContextFactory>();
            services.AddSingleton<IDatabaseGateway, DatabaseGateway>();
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            services.AddSingleton<IDbCommandFactory, DbCommandFactory>();
            services.AddSingleton<IDataRowMapper, DataRowMapper>();
            services.AddSingleton<IQueryMapper, QueryMapper>();
            services.AddSingleton<ISessionQueryFactory, SessionQueryFactory>();
            services.AddSingleton<ISessionQuery, SessionQuery>();
            services.AddSingleton<ISessionRepository, SessionRepository>();
            services.AddSingleton<IDataRowMapper<Session>, SessionMapper>();
            services.AddSingleton(typeof(IDataRepository<>), typeof(DataRepository<>));
            services.AddSingleton<IAccessService, DataStoreAccessService>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Shuttle.Access.WebApi", Version = "v1" });

                options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme()
                {
                    Name = "access-session-token",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Description = "Shuttle.Access token security",
                    Scheme = "ApiKeyScheme"
                });

                var key = new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {
            var container = app.ApplicationServices.GetService<IWindsorContainer>();

            var componentContainer = new WindsorComponentContainer(container);

            componentContainer.RegisterSuffixed("Shuttle.Access.Sql");

            componentContainer.Register<IHttpContextAccessor, HttpContextAccessor>();
            componentContainer.Register<IDatabaseContextCache, ContextDatabaseContextCache>();
            componentContainer.Register<IHashingService, HashingService>();
            componentContainer.Register<IPasswordGenerator, DefaultPasswordGenerator>();

            componentContainer.RegisterInstance(app.ApplicationServices.GetService<IAccessConfiguration>());

            var applicationPartManager = app.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            var controllerFeature = new ControllerFeature();

            applicationPartManager.PopulateFeature(controllerFeature);

            foreach (var type in controllerFeature.Controllers.Select(t => t.AsType()))
            {
                componentContainer.Register(type, type);
            }

            componentContainer.RegisterDataAccess();
            componentContainer.RegisterServiceBus();
            componentContainer.RegisterEventStore();
            componentContainer.RegisterEventStoreStorage();
            componentContainer.RegisterMediator();
            componentContainer.RegisterMediatorParticipants(Assembly.Load("Shuttle.Access.Application"));

            componentContainer.Register<IAzureStorageConfiguration, DefaultAzureStorageConfiguration>();

            var databaseContextFactory = componentContainer.Resolve<IDatabaseContextFactory>();

            databaseContextFactory.ConfigureWith("Access");

            _bus = componentContainer.Resolve<IServiceBus>().Start();

            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler(error =>
            {
                error.Run(async context =>
                {
                    var feature = context.Features.Get<IExceptionHandlerFeature>();

                    if (feature != null)
                    {
                        _log.Error(feature.Error.AllMessages());
                    }

                    await Task.CompletedTask;
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
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shuttle.Access.WebApi.v1");
            });
        }
    }
}