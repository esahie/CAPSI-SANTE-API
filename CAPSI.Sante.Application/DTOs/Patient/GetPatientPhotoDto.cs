using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Patient
{
    public class GetPatientPhotoDto
    {
        public Guid PatientId { get; set; }
        public string PhotoUrl { get; set; }
        public string NomComplet { get; set; }
        public bool HasPhoto => !string.IsNullOrEmpty(PhotoUrl);
        public DateTime? LastUpdated { get; set; }
    }
}
