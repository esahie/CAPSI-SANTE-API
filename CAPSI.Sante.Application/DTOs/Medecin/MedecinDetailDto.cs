using CAPSI.Sante.Application.DTOs.ServiceMedical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Medecin
{
    public class MedecinDetailDto : MedecinBaseDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ServiceMedicalDto> ServicesOfferts { get; set; } = new List<ServiceMedicalDto>();
    }
}
