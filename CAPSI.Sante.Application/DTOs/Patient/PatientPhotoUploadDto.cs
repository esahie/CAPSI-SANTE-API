using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Patient
{
    public class PatientPhotoUploadDto
    {
        [Required(ErrorMessage = "L'ID du patient est requis")]
        public Guid PatientId { get; set; }

        [Required(ErrorMessage = "Le fichier photo est requis")]
        public IFormFile Photo { get; set; }
    }
}
