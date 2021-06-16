using System;
using System.Globalization;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GameUser.Web.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class InternationalizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string culture = (string)context.RouteData.Values["culture"] ?? "en-US";

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
        }
    }
}