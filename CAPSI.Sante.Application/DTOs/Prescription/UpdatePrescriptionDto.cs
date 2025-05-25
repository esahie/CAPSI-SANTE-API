using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs.Prescription
{
    public class UpdatePrescriptionDto
    {
        public Guid Id { get; set; }
        public DateTime? DateFin { get; set; }
        public string Instructions { get; set; }
        public List<MedicamentPrescritDto> Medicaments { get; set; } = new List<MedicamentPrescritDto>();
    }
}
