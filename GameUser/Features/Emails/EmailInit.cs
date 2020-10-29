using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using System;

namespace GameUser.Features.Emails
{
    public static class EmailInit
    {
        public static void InitEmail(this IServiceCollection services, IConfiguration configuration)
        {            
            var emailConfigs = new EmailConfigs(configuration);
            services.AddSingleton<IEmailConfigs>(emailConfigs);

            services.AddMailKit(optionBuilder =>
            {
                var mailKitOptions = new MailKitOptions()
                {
                    Server = emailConfigs.Server,
                    Port = emailConfigs.Port,
                    SenderName = emailConfigs.SenderName,
                    SenderEmail = emailConfigs.SenderEmail,
                    Account = emailConfigs.Account,
                    Password = emailConfigs.Password,
                    Security = emailConfigs.Security                  
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
        }
    }
}
