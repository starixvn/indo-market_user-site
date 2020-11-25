using GameUser.Web.Shared.Attributes;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace GameUser.Web.Features.Feedbacks.ViewModels
{
    public class FeedbackViewModel
    {
        public string Category { get; set; }

        public string Description { get; set; }

        [MaxFileSize(15728640)]
        [AllowedExtensions(new[] { ".jpg", ".png", ".jpeg", ".gif" })]
        public List<IFormFile> Files { get; set; }
    }
}
