using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GameUser.Web.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class InternationalizationAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string culture = (string)context.RouteData.Values["culture"] ?? "en-US";

            if (culture == "zh-CN") {
                culture = "zh-Hans";
            }

            if (culture == "zh-TW")
            {
                culture = "zh-Hant";
            }

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(culture);

        }       
    }
}
