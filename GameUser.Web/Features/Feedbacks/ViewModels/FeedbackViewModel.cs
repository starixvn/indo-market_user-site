using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace GameUser.Web.Features.Feedbacks.ViewModels
{
    public class FeedbackViewModel
    {
        public string Category { get; set; }
        public string Description { get; set; }
        public List<IFormFile> Files { get; set; }
    }
}
