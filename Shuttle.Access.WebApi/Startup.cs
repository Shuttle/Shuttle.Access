using System.Linq;
using Castle.Windsor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Sql;
using Shuttle.Core.Castle;
using Shuttle.Core.Container;
using Shuttle.Core.Data;
using Shuttle.Core.Data.Http;
using Shuttle.Core.Logging;
using Shuttle.Esb;
using Shuttle.Recall;

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

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IHostApplicationLifetime applicationLifetime)
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

            ServiceBus.Register(componentContainer);
            EventStore.Register(componentContainer);

            var databaseContextFactory = componentContainer.Resolve<IDatabaseContextFactory>();
            var roleQuery = componentContainer.Resolve<ISystemRoleQuery>();

            databaseContextFactory.ConfigureWith("Access");

            _bus = ServiceBus.Create(componentContainer).Start();

            bool administratorExists;

            using (databaseContextFactory.Create())
            {
                administratorExists =
                    roleQuery.Count(new DataAccess.Query.Role.Specification().WithRoleName("Administrator")) > 0;

                if (!administratorExists)
                {
                    _bus.Send(new AddRoleCommand
                    {
                        Name = "Administrator"
                    });
                }
            }

            _log.Information(
                $"[role] : name = 'Administrator' / exists = {administratorExists}{(administratorExists ? string.Empty : " / add role command sent")}");

            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
        }
    }
}