using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Patient
{
    public class RequestReactivationDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(500)]
        public string MotifDemande { get; set; }
    }
}
