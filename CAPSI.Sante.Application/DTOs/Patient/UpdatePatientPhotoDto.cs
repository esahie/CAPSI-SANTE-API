using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Patient
{
    public class UpdatePatientPhotoDto
    {
        [Required(ErrorMessage = "L'ID du patient est requis")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "L'URL de la photo est requise")]
        [StringLength(500, ErrorMessage = "L'URL ne peut pas dépasser 500 caractères")]
        [Url(ErrorMessage = "L'URL de la photo n'est pas valide")]
        public string? PhotoUrl { get; set; }
    }
}
