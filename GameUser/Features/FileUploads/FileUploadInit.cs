using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameUser.Features.FileUploads
{
    public static class FileUploadInit
    {
        public static void InitFileUpload(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IFileUploadService, FileUploadService>();
        }
    }
}
