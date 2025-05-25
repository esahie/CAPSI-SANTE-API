using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Medecin
{
    public abstract class MedecinBaseDto
    {
        [Required]
        [StringLength(50)]
        public string NumeroLicence { get; set; }

        public Guid? UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; }

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; }

        [Required]
        [StringLength(100)]
        public string Specialite { get; set; }

        [Phone]
        public string Telephone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [StringLength(255)]
        public string AdresseCabinet { get; set; }

        [StringLength(50)]
        public string CodePostal { get; set; }

        [StringLength(100)]
        public string Ville { get; set; }

        // Nouvelles propriétés pour la photo
        public string PhotoUrl { get; set; }
        public string PhotoNom { get; set; }
        public string PhotoType { get; set; }
        public long? PhotoTaille { get; set; }
    }
}
