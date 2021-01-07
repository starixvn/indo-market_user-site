using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Sentry;

namespace GameUser.Web
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var host = CreateWebHostBuilder(args).Build();

                SentrySdk.CaptureMessage(string.Format("{0} has been started at {1}", typeof(Program).FullName, DateTime.Now));

                host.Run();
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e);
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                   .Build();

            return WebHost.CreateDefaultBuilder(args)
               .UseKestrel()
               .UseUrls($"http://*:{configuration["HostingPort"]}")
               .UseSentry()
               .UseIISIntegration()
               .UseStartup<Startup>();
        }
    }
}