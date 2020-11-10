using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace GameUser.Features.Feedbacks
{
    public class Feedback
    {
        public Feedback(){}

        public Feedback(string category, string description, string nickname, List<IFormFile> files)
        {
            this.Category = category;
            this.Description = description;
            this.Nickname = nickname;
            this.Files = files;
        }

        public string Category { get; set; }
        public string Description { get; set; }
        public string Nickname { get; set; }
        public List<IFormFile> Files { get; set; }
    }
}
