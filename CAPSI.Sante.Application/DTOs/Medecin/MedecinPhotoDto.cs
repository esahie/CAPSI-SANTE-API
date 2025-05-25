using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Medecin
{
    public class MedecinPhotoDto
    {
        [Required]
        public Guid MedecinId { get; set; }

        [Required]
        public IFormFile Photo { get; set; }
    }
}
