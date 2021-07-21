using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace GameUser.Web.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int maxFileSize;

        public MaxFileSizeAttribute(int maxFileSize)
        {
            this.maxFileSize = maxFileSize;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var files = value as IList<IFormFile>;

            if (files != null && files.Any())
            {
                foreach (var file in files)
                {
                    if (file.Length > maxFileSize)
                    {
                        return new ValidationResult(GetErrorMessage());
                    }
                }
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"Maximum allowed file size is { maxFileSize} bytes.";
        }
    }
}