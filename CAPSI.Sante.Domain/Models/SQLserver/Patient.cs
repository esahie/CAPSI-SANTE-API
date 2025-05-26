using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.SQLserver
{
    public class Patient
    {
        public Guid Id { get; set; }
        public string NumeroAssuranceMaladie { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public DateTime DateNaissance { get; set; }
        public string Sexe { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Adresse { get; set; }
        public string CodePostal { get; set; }
        public string Ville { get; set; }
        public string GroupeSanguin { get; set; }

        // Nouvelle propriété pour la photo
        public string PhotoUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UserId { get; set; }
        public bool EstActif { get; set; } = true;

        // Navigation property
        public DossierMedical DossierMedical { get; set; }

        // Propriétés calculées pour l'interface utilisateur
        public string NomComplet => $"{Prenom} {Nom}";
        public int Age => DateTime.Today.Year - DateNaissance.Year -
                         (DateTime.Today.DayOfYear < DateNaissance.DayOfYear ? 1 : 0);
        public bool APhoto => !string.IsNullOrEmpty(PhotoUrl);
    }
}
