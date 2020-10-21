using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace UserSite.Api
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                   .Build();

            return WebHost.CreateDefaultBuilder(args)
               .UseStartup<Startup>()
               .UseSentry(opt => opt.Environment = configuration["Env"])
               .UseUrls($"http://*:{configuration["HostingPort"]}");
        }
    }
}
