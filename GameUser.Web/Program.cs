using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Sentry;
using System;
using System.IO;

namespace GameUser.Web
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (environment == "Development")
                {
                    SentrySdk.CaptureMessage(string.Format("{0} has been started at {1}", typeof(Program).FullName, DateTime.Now));
                }
                
                CreateWebHostBuilder(args).Build().Run();
            }
            catch(Exception e)
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
               .UseStartup<Startup>()
               .UseSentry(opt => opt.Environment = configuration["Env"])
               .UseIISIntegration()
               .UseUrls($"http://*:{configuration["HostingPort"]}");
        }
    }
}
