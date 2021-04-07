using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;

namespace GameUser.Web.Shared.Middlewares
{
    public static class CustomRewriterMiddleware
    {
        public static IApplicationBuilder UseCustomRewriter(this IApplicationBuilder app)
        {
            var options = new RewriteOptions()
                .AddRedirect("zh-CN/(.*)", "zh-Hans/$1")
                .AddRedirect("zh-TW/(.*)", "zh-Hant/$1");

            return app.UseRewriter(options);
        }
    }
}
