using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Patient
{
    public class CreatePatientDto
    {
        [Required]
        [StringLength(50)]
        public string NumeroAssuranceMaladie { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; }

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; }

        [Required]
        public DateTime DateNaissance { get; set; }

        [Required]
        [StringLength(1)]
        [RegularExpression("^[MF]$")]
        public string Sexe { get; set; }

        [Phone]
        public string Telephone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [StringLength(255)]
        public string Adresse { get; set; }

        [StringLength(10)]
        public string CodePostal { get; set; }

        [StringLength(100)]
        public string Ville { get; set; }

        [StringLength(5)]
        [RegularExpression("^(A|B|AB|O)[+-]$")]
        public string GroupeSanguin { get; set; }

        public Guid? UserId { get; set; }
    }
}
