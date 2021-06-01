namespace GameUser.Web
{
    using AutoMapper;
    using GameUser.Features.BackendAuthentications;
    using GameUser.Shared.SessionFilterAuthentications;
    using GameUser.Web.Shared.Configurations;
    using GameUser.Web.Shared.Middlewares;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Rewrite;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
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
            services.AddAppSettings();
            services.ConfigMongoDB(Configuration);
            services.AddLocalizationSettings(Configuration);

            services.AddHttpContextAccessor();
            services.AddHealthChecks();
            services.ConfigGameUser(Configuration);

            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc(name: "v1", new OpenApiInfo { Title = "User APIs", Version = "v1" });
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                 .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                 {
                     options.ExpireTimeSpan = TimeSpan.FromHours(1);
                     options.LoginPath = "/Error/NotAuthorized";
                     options.AccessDeniedPath = "/Error/NotAuthorized";
                 })
                 .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                 {
                     options.Authority = Configuration["BackendAuthenticationSettings:Address"];
                     options.RequireHttpsMetadata = false;
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidAudiences = Configuration.GetSection("BackendAuthenticationSettings:Audiences").Get<string[]>()
                     };
                 })
                 .AddScheme<SessionFilterAuthenticationOptions, SessionFilterAuthenticationHandler>(SessionFilterAuthenticationDefaults.AuthenticationScheme, null);

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            app.UseSwagger();
            app.UseSwaggerUI(swagger =>
            {
                swagger.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name: "User APIs v1");
            });

            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRequestLocalization();

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseCustomRewriter();
            app.UseBackendAuthenticationTokenStore();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "culture-route",
                    pattern: "{culture=en-US}/{controller=Feedback}/{action=Add}");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Feedback}/{action=Add}");
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}