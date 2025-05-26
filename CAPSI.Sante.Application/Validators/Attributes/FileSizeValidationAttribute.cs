using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Validators.Attributes
{
    public class FileSizeValidationAttribute : ValidationAttribute
    {
        private readonly long _maxSizeInBytes;

        public FileSizeValidationAttribute(int maxSizeInMB)
        {
            _maxSizeInBytes = maxSizeInMB * 1024 * 1024;
        }

        public override bool IsValid(object value)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxSizeInBytes)
                {
                    ErrorMessage = $"La taille du fichier ne doit pas dépasser {_maxSizeInBytes / (1024 * 1024)} MB";
                    return false;
                }
            }

            return true;
        }
    }
}
