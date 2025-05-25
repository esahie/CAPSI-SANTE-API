using CAPSI.Sante.Application.DTOs.Prescription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.DTOs
{
    public class PrescriptionDto
    {
        public Guid Id { get; set; }
        public Guid DossierId { get; set; }
        public Guid MedecinId { get; set; }
        public string NomMedecin { get; set; }
        public string PrenomMedecin { get; set; }
        public DateTime DatePrescription { get; set; }
        public DateTime? DateFin { get; set; }
        public string Instructions { get; set; }
        public List<MedicamentPrescritDto> Medicaments { get; set; } = new List<MedicamentPrescritDto>();
    }
}
