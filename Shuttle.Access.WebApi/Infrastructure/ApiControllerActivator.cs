using System;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Castle.Windsor;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Access.WebApi
{
	public class ApiControllerActivator : IHttpControllerActivator
	{
		private readonly IWindsorContainer _container;

		public ApiControllerActivator(IWindsorContainer container)
		{
			Guard.AgainstNull(container, "container");

			_container = container;
		}

		public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor,
									  Type controllerType)
		{
			Guard.AgainstNull(controllerType, "controllerType");

			try
			{
				return _container.Resolve<IHttpController>(controllerType.Name);
			}
			catch (Exception ex)
			{
				throw new HttpException(500, $"The controller for path '{request.RequestUri.AbsolutePath}' could not be instanced.  Exception: {ex.AllMessages()}");
			}
		}
		 
	}
}