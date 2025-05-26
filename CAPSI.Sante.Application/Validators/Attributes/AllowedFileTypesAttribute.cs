using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Validators.Attributes
{
    public class AllowedFileTypesAttribute : ValidationAttribute
    {
        private readonly string[] _allowedTypes;

        public AllowedFileTypesAttribute(params string[] allowedTypes)
        {
            _allowedTypes = allowedTypes;
        }

        public override bool IsValid(object value)
        {
            if (value is IFormFile file)
            {
                if (file == null || file.Length == 0)
                    return true; // Les fichiers vides sont autorisés (optionnel)

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var contentType = file.ContentType.ToLowerInvariant();

                // Vérifier l'extension ET le type MIME
                var isValidExtension = _allowedTypes.Any(type =>
                    type.StartsWith(".") ? type == fileExtension : type == contentType);

                if (!isValidExtension)
                {
                    ErrorMessage = $"Type de fichier non autorisé. Types autorisés : {string.Join(", ", _allowedTypes)}";
                    return false;
                }
            }

            return true;
        }
    }
}
