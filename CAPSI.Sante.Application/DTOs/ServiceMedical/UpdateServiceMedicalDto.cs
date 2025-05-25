using System;
using System.ComponentModel.DataAnnotations;

namespace CAPSI.Sante.Application.DTOs.ServiceMedical
{
    public class UpdateServiceMedicalDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Code { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Nom { get; set; }

        [StringLength(4000)]
        public string Description { get; set; }

        [Range(5, 480)]
        public int DureeParDefaut { get; set; }

        [Range(0, 10000)]
        public decimal Tarif { get; set; }

        public bool RequiertAssurance { get; set; }

        public bool EstActif { get; set; } = true;
    }
}