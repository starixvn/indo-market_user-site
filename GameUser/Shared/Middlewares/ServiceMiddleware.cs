namespace GameUser.Shared.Middlewares
{
    using GameUser.Features.Feedbacks;
    using GameUser.Features.FileUploads;
    using GameUser.Features.Mail;
    using GameUser.Shared.Repositories;
    using Microsoft.Extensions.DependencyInjection;
    using NETCore.MailKit.Extensions;
    using NETCore.MailKit.Infrastructure.Internal;
    using System;

    public static class ServiceMiddleware
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<IFileUploadService, FileUploadService>();
        }

        public static IServiceCollection AddEmail(this IServiceCollection services, IEmailConfigs configuration)
        {
            services.AddMailKit(optionBuilder =>
            {
                var mailKitOptions = new MailKitOptions()
                {
                    Server = configuration.Server,
                    Port = configuration.Port,
                    SenderName = configuration.SenderName,
                    SenderEmail = configuration.SenderEmail,
                    Account = configuration.Account,
                    Password = configuration.Password,
                    Security = configuration.Security
                    //Server = configuration.GetValue<string>("Email:Server"),
                    //Port = configuration.GetValue<int>("Email:Port"),
                    //SenderName = configuration.GetValue<string>("Email:SenderName"),
                    //SenderEmail = configuration.GetValue<string>("Email:SenderEmail"),
                    //// can be optional with no authentication 
                    //Account = configuration.GetValue<string>("Email:Account"),
                    //Password = configuration.GetValue<string>("Email:Password"),
                    //Security = configuration.GetValue<bool>("Email:Security")
                };

                if (mailKitOptions.Server == null)
                {
                    throw new InvalidOperationException("Please specify SmtpServer in appsettings");
                }
                if (mailKitOptions.Port == 0)
                {
                    throw new InvalidOperationException("Please specify Smtp port in appsettings");
                }

                if (mailKitOptions.SenderEmail == null)
                {
                    throw new InvalidOperationException("Please specify SenderEmail in appsettings");
                }

                optionBuilder.UseMailKit(mailKitOptions);
            });

            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}
