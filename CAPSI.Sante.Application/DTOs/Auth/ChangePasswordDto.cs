using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Auth
{
    public class ChangePasswordDto
    {
        [JsonIgnore] // <- Empêche d'être attendu dans les requêtes
        public string Email { get; set; }

        [Required(ErrorMessage = "Le mot de passe actuel est requis.")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Le nouveau mot de passe est requis.")]
        [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "La confirmation du mot de passe est requise.")]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string ConfirmNewPassword { get; set; }
    }

}
