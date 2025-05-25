using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Documents
{
    public class UpdateDocumentDto
    {
        [Required]
        [StringLength(255)]
        public string Titre { get; set; }

        [Required]
        public string Type { get; set; }
    }
}
