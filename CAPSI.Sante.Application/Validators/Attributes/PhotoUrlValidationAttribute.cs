using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Validators.Attributes
{
    public class PhotoUrlValidationAttribute : ValidationAttribute
    {
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

        public override bool IsValid(object value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return true; // Les valeurs nulles sont autorisées (optionnel)

            var url = value.ToString();

            // Vérifier si c'est une URL valide
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
            {
                // Si ce n'est pas une URL absolue, vérifier si c'est un chemin local valide
                if (!url.StartsWith("/uploads/"))
                {
                    ErrorMessage = "L'URL de la photo doit être une URL valide ou un chemin local commençant par /uploads/";
                    return false;
                }
            }
            else
            {
                // Vérifier le schéma pour les URLs absolues
                if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
                {
                    ErrorMessage = "L'URL de la photo doit utiliser le protocole HTTP ou HTTPS";
                    return false;
                }
            }

            // Vérifier l'extension du fichier
            var extension = Path.GetExtension(url).ToLowerInvariant();
            if (!string.IsNullOrEmpty(extension) && !_allowedExtensions.Contains(extension))
            {
                ErrorMessage = $"L'extension du fichier photo n'est pas autorisée. Extensions autorisées : {string.Join(", ", _allowedExtensions)}";
                return false;
            }

            return true;
        }
    }
}
