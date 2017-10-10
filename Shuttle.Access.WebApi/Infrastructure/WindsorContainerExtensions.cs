using System;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Access.WebApi
{
    public static class WindsorContainerExtensions
    {
        public static void RegisterDataAccess(this IWindsorContainer container, string assemblyName)
        {
            Guard.AgainstNull(container, "container");

            container.RegisterDataAccess(Assembly.Load(assemblyName));
        }

        public static void RegisterDataAccess(this IWindsorContainer container, Assembly assembly)
        {
            Guard.AgainstNull(container, "container");

            container.Register(assembly, typeof(IDataRowMapper<>));

            container.Register(assembly, "Repository");
            container.Register(assembly, "Query");
            container.Register(assembly, "Factory");
        }

        public static void Register(this IWindsorContainer container, string assemblyName, string endsWith)
        {
            container.Register(Assembly.Load(assemblyName), endsWith);
        }

        public static void Register(this IWindsorContainer container, Assembly assembly, string endsWith)
        {
            Guard.AgainstNull(container, "container");
            Guard.AgainstNull(assembly, "assembly");
            Guard.AgainstNullOrEmptyString(endsWith, "endsWith");

            container.Register(
                Classes
                    .FromAssembly(assembly)
                    .Pick()
                    .If(type => type.Name.EndsWith(endsWith))
                    .WithServiceFirstInterface()
                    .Configure(c => c.Named(c.Implementation.UnderlyingSystemType.Name)));
        }

        public static void Register(this IWindsorContainer container, string assemblyName, Type type)
        {
            container.Register(Assembly.Load(assemblyName), type);
        }

        public static void Register(this IWindsorContainer container, Assembly assembly, Type type)
        {
            Guard.AgainstNull(container, "container");
            Guard.AgainstNull(assembly, "assembly");
            Guard.AgainstNull(type, "type");

            container.Register(
                Classes
                    .FromAssembly(assembly)
                    .BasedOn(type)
                    .WithServiceFirstInterface()
                    .Configure(c => c.Named(c.Implementation.UnderlyingSystemType.Name)));
        }

        public static void Register(this IWindsorContainer container, string assemblyName, Type type,
            string endsWith)
        {
            container.Register(Assembly.Load(assemblyName), type, endsWith);
        }

        public static void Register(this IWindsorContainer container, Assembly assembly, Type type, string endsWith)
        {
            Guard.AgainstNull(container, "container");
            Guard.AgainstNull(assembly, "assembly");
            Guard.AgainstNull(type, "type");
            Guard.AgainstNullOrEmptyString(endsWith, "endsWith");

            container.Register(
                Classes
                    .FromAssembly(assembly)
                    .Pick()
                    .If(candidate => candidate.Name.EndsWith(endsWith, StringComparison.OrdinalIgnoreCase)
                                     &&
                                     type.IsAssignableFrom(candidate))
                    .LifestyleTransient()
                    .WithServiceFirstInterface()
                    .Configure(c => c.Named(c.Implementation.UnderlyingSystemType.Name)));
        }
    }
}