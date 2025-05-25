using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.ServiceMedical
{
    public class CreateServiceMedicalDto
    {
        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(15, 180)]
        public int DureeParDefaut { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal Tarif { get; set; }

        public bool RequiertAssurance { get; set; } = true;
    }
}
