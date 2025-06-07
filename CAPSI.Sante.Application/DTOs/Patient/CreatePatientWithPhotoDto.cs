using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Patient
{
    public class CreatePatientWithPhotoDto
    {
        [Required]
        public string NumeroAssuranceMaladie { get; set; }

        [Required]
        public string Nom { get; set; }

        [Required]
        public string Prenom { get; set; }

        [Required]
        public DateTime DateNaissance { get; set; }

        [Required]
        [RegularExpression("^[MF]$")]
        public string? Sexe { get; set; }

        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Adresse { get; set; }
        public string? CodePostal { get; set; }
        public string? Ville { get; set; }

        [RegularExpression("^(A|B|AB|O)[+-]$")]
        public string? GroupeSanguin { get; set; }

        public Guid? UserId { get; set; }

        [Required]
        public IFormFile? Photo { get; set; }

        public string? PhotoNom { get; set; }
        public string? PhotoType { get; set; }
        public long? PhotoTaille { get; set; }

    }

}
