using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace GameUser.Features.Feedbacks
{
    public class Feedback
    {
        public Feedback()
        {
        }

        public Feedback(string category, string description, List<IFormFile> files)
        {
            this.Category = category;
            this.Description = description;
            this.Files = files;
        }

        public string Category { get; }
        public string Description { get; }
        public List<IFormFile> Files { get; }
    }
}