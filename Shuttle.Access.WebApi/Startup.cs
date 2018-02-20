using System;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Castle;
using Shuttle.Core.Container;
using Shuttle.Esb;

namespace Shuttle.Access.WebApi
{
    public class Startup : IDisposable
    {
        private IServiceBus _bus;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void Dispose()
        {
            _bus?.Dispose();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddCors();

            var container = new WindsorContainer();

            var componentContainer = new WindsorComponentContainer(container);

            componentContainer.RegisterSuffixed("Shuttle.Access.Sql");

            ServiceBus.Register(componentContainer);

            _bus = ServiceBus.Create(componentContainer).Start();

            return WindsorRegistrationHelper.CreateServiceProvider(container, services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(
                options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
            );

            app.UseMvc();
        }
    }
}