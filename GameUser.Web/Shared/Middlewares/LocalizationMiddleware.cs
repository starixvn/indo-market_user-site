using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Web.Shared.Middlewares
{
    public static class LocalizationMiddleware
    {
        public static void AddLocalizationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var cultures = configuration.GetSection("SupportedCultures").GetChildren().Select(x => new CultureInfo(x.Value)).ToList();

            services.Configure<RequestLocalizationOptions>(opts =>
            {
                opts.DefaultRequestCulture = new RequestCulture(configuration["DefaultCulture"]);
                opts.SupportedCultures = cultures;
                opts.SupportedUICultures = cultures;
                opts.RequestCultureProviders.Insert(0, new RouteDataRequestCultureProvider { Options = opts });
            });

            services.AddLocalization(opts =>
            {
                opts.ResourcesPath = "";
            });
        }
    }
}