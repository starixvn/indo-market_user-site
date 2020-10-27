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
        Task<IEnumerable<UploadedFile>> UploadFiles(List<IFormFile> files, string baseDirectory);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly ISentryClient sentryClient;

        public FileUploadService(ISentryClient sentryClient)
        {
            this.sentryClient = sentryClient;
        }

        public async Task<IEnumerable<UploadedFile>> UploadFiles(List<IFormFile> files, string baseDirectory)
        {
            var result = new List<UploadedFile>();
            if (files != null)
            {
                Directory.CreateDirectory(baseDirectory);
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

                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

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
