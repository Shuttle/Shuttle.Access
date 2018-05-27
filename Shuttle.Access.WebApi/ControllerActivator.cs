using Castle.Windsor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Shuttle.Core.Contract;

namespace Shuttle.Access.WebApi
{
    public class ControllerActivator : IControllerActivator
    {
        private readonly IWindsorContainer _container;

        public ControllerActivator(IWindsorContainer container)
        {
            Guard.AgainstNull(container, nameof(container));

            _container = container;
        }

        public object Create(ControllerContext context)
        {
            return _container.Resolve(context.ActionDescriptor.ControllerTypeInfo.AsType());
        }

        public void Release(ControllerContext context, object controller)
        {
            _container.Release(controller);
        }
    }
}