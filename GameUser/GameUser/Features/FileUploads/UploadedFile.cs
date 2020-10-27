using Microsoft.AspNetCore.Http;

namespace GameUser.Features.FileUploads
{
    public class UploadedFile
    {
        public IFormFile File { get; set; }
        public string Name { get; set; }
        public string FullPath { get; set; }
        public bool SavedSuccess { get; set; }
    }
}
