using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Core.DTOs
{
    public class AnnulationDto
    {
        [Required]
        [StringLength(500)]
        public string Motif { get; set; }
    }
}
