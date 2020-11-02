namespace GameUser.Web
{
    using AutoMapper;
    using GameUser.Shared.Middlewares;
    using GameUser.Web.Shared;
    using GameUser.Web.Shared.Middlewares;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));
            services.AddAppSettings(Configuration);
            services.ConfigMongoDB(Configuration);

            services.ConfigGameUser(Configuration);            

            services.AddMvc(o => o.Conventions.Add(new FeatureConvention()))
                  .AddRazorOptions(options =>
                  {
                      // {0} - Action Name
                      // {1} - Controller Name
                      // {2} - Area Name
                      // {3} - Feature Name
                      // Replace normal view location entirely
                      options.ViewLocationFormats.Clear();
                      options.ViewLocationFormats.Add("/Features/{3}/{1}/{0}.cshtml");
                      options.ViewLocationFormats.Add("/Features/{3}/Views/{0}.cshtml");
                      options.ViewLocationFormats.Add("/Features/{3}/{0}.cshtml");
                      options.ViewLocationFormats.Add("/Features/_Shared/{0}.cshtml");
                      options.ViewLocationFormats.Add("/Features/_Shared/Components/{0}.cshtml");
                      options.ViewLocationExpanders.Add(new FeatureViewLocationExpander());
                  });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                 .AddCookie(options =>
                 {
                     options.ExpireTimeSpan = TimeSpan.FromHours(1);
                     options.LoginPath = "/Error/NotAuthorized";
                     options.AccessDeniedPath = "/Error/NotAuthorized";
                 });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/NotAuthorized");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Feedback}/{action=Add}");
            });
        }
    }
}
