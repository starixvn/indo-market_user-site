using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GameUser.Shared.Wrappers
{
    public interface ISystemIOWrapper
    {
        Task WriteFile(IFormFile file, string filePath);

        DirectoryInfo CreateDirectory(string path);
    }

    public class SystemIOWrapper : ISystemIOWrapper
    {
        public DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }

        public async Task WriteFile(IFormFile file, string filePath)
        {
            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
        }
    }
}