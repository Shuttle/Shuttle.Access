using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Ninject;
using Shuttle.Core.Contract;

namespace Shuttle.Access.WebApi
{
    public class ControllerActivator : IControllerActivator
    {
        private readonly IKernel _kernel;

        public ControllerActivator(IKernel kernel)
        {
            Guard.AgainstNull(kernel, nameof(kernel));

            _kernel = kernel;
        }

        public object Create(ControllerContext context)
        {
            return _kernel.Get(context.ActionDescriptor.ControllerTypeInfo.AsType());
        }

        public void Release(ControllerContext context, object controller)
        {
            _kernel.Release(controller);
        }
    }
}