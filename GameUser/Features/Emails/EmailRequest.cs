using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameUser.Features.Emails
{
    public class EmailRequest
    {
        public EmailRequest(string toAddress, string body, string subject, IList<IFormFile> attachments)
        {
            this.ToAddress = toAddress;
            this.Body = body;
            this.Subject = subject;
            this.Attachments = attachments;
        }

        [Required]
        public string ToAddress { get; }

        [Required]
        public string Body { get; }

        [Required]
        public string Subject { get; }

        public IList<IFormFile> Attachments { get; }
    }
}