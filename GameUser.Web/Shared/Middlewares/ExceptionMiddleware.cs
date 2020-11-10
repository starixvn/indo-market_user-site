namespace GameUser.Web.Shared.Middlewares
{
    using GameUser.Web.Shared.Models;
    using Microsoft.AspNetCore.Http;
    using Sentry;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);

                await HandleExceptionAsync(httpContext);
            }
        }

        private Task HandleExceptionAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            return context.Response.WriteAsync(new ErrorDetails(
                statusCode: context.Response.StatusCode, 
                message: "Internal Server Error from the custom middleware.").ToString());
        }
    }
}
