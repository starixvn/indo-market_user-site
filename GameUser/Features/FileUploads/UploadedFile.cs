using Microsoft.AspNetCore.Http;

namespace GameUser.Features.FileUploads
{
    public class UploadedFile
    {
        public UploadedFile(IFormFile file, string name, string fullPath, bool savedSuccess)
        {
            this.File = file;
            this.Name = name;
            this.FullPath = fullPath;
            this.SavedSuccess = savedSuccess;
        }

        public IFormFile File { get; }
        public string Name { get; }
        public string FullPath { get; }
        public bool SavedSuccess { get; }
    }
}