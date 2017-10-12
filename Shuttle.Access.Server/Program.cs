using Castle.Windsor;
using log4net;
using Shuttle.Core.Castle;
using Shuttle.Core.Castle.Extensions;
using Shuttle.Core.Infrastructure;
using Shuttle.Core.Log4Net;
using Shuttle.Core.ServiceHost;
using Shuttle.Recall;

namespace Shuttle.Access.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ServiceHost.Run<Host>();
        }
    }

}