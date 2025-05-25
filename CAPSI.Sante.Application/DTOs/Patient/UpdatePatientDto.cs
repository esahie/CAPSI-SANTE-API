using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Patient
{
    public class UpdatePatientDto : CreatePatientDto
    {
        [Required]
        public Guid Id { get; set; }
    }
}
