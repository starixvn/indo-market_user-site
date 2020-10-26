using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameUser.Features.Mail
{
    public class EmailRequest
    {
        [Required]
        public string ToAddress { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public string Subject { get; set; }

        public IList<IFormFile> Attachments { get; set; }
    }
}
