using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.SQLserver
{
    public class User
    {
        // Utiliser le bon nom de propriété selon votre base de données
        public Guid UserId { get; set; } // Si votre colonne s'appelle UserId

        public string Email { get; set; }
        public string PasswordHash { get; set; }

        // Utiliser le bon nom de propriété selon votre base de données
        public string UserType { get; set; } // "Admin", "Medecin", "Patient"

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        //// Propriétés de mapping pour la compatibilité
        //public Guid Id => UserId;
        //public string Role => UserType;
    }
}
