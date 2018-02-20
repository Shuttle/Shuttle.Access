using System;
using System.IO;
using log4net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Shuttle.Core.Log4Net;
using Shuttle.Core.Logging;

namespace Shuttle.Access.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Assign(
                new Log4NetLog(LogManager.GetLogger(typeof(Program)),
                    new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.xml"))));

            Log.Information("[started]");

            BuildWebHost(args).Run();

            Log.Information("[stopped]");

            LogManager.Shutdown();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
        }
    }
}