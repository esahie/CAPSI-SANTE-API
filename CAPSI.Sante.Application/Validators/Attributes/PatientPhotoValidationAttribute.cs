using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Validators.Attributes
{
    public class PatientPhotoValidationAttribute : ValidationAttribute
    {
        private readonly long _maxSizeInBytes = 5 * 1024 * 1024; // 5MB
        private readonly string[] _allowedTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif" };
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public override bool IsValid(object value)
        {
            if (value == null)
                return true; // Les valeurs nulles sont autorisées

            if (value is IFormFile file)
            {
                // Vérifier si le fichier est vide
                if (file.Length == 0)
                {
                    ErrorMessage = "Le fichier photo ne peut pas être vide";
                    return false;
                }

                // Vérifier la taille
                if (file.Length > _maxSizeInBytes)
                {
                    ErrorMessage = $"La photo ne doit pas dépasser {_maxSizeInBytes / (1024 * 1024)} MB";
                    return false;
                }

                // Vérifier le type MIME
                if (!_allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
                {
                    ErrorMessage = "Type de fichier non autorisé. Seuls les fichiers JPG, PNG et GIF sont acceptés";
                    return false;
                }

                // Vérifier l'extension
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                {
                    ErrorMessage = "Extension de fichier non autorisée";
                    return false;
                }

                return true;
            }

            // Si c'est une string (URL)
            if (value is string url)
            {
                if (string.IsNullOrEmpty(url))
                    return true;

                // Validation de l'URL
                var photoUrlValidator = new PhotoUrlValidationAttribute();
                return photoUrlValidator.IsValid(url);
            }

            return false;
        }
    }
}
