using GameUser.Shared.Wrapper;
using Microsoft.AspNetCore.Http;
using Sentry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameUser.Features.FileUploads
{
    public interface IFileUploadService
    {
        Task<IEnumerable<UploadedFile>> UploadFilesAsync(List<IFormFile> files, string baseDirectory);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly ISentryClient sentryClient;
        private readonly ISystemIOWrapper systemIOWrapper;

        public FileUploadService(ISentryClient sentryClient, ISystemIOWrapper systemIOWrapper)
        {
            this.sentryClient = sentryClient;
            this.systemIOWrapper = systemIOWrapper;
        }

        public async Task<IEnumerable<UploadedFile>> UploadFilesAsync(List<IFormFile> files, string baseDirectory)
        {
            var result = new List<UploadedFile>();
            if (files != null)
            {
                systemIOWrapper.CreateDirectory(baseDirectory);
                foreach (var file in files)
                {
                    var r = await SaveFile(file, baseDirectory);
                    result.Add(r);
                }
            }

            return result;
        }

        private async Task<UploadedFile> SaveFile(IFormFile file, string baseDirectory)
        {
            var uploadedFile = new UploadedFile { File = file };
            try
            {
                var name = Guid.NewGuid().ToString();
                var filePath = Path.Combine(baseDirectory, name);

                await systemIOWrapper.WriteFile(file, filePath);                

                uploadedFile.Name = name;
                uploadedFile.FullPath = filePath;
                uploadedFile.SavedSuccess = true;
            }
            catch(Exception e)
            {
                sentryClient.CaptureException(e);
                uploadedFile.SavedSuccess = false;
            }

            return uploadedFile;
        }
    }
}
