using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.SQLserver
{
    public class Prescription
    {
        public Guid Id { get; set; }
        public Guid DossierId { get; set; }
        public Guid MedecinId { get; set; }
        public DateTime DatePrescription { get; set; }
        public DateTime? DateFin { get; set; }
        public string Instructions { get; set; }

        // Propriétés de navigation (non stockées en base)
        public DossierMedical Dossier { get; set; }
        public Medecin Medecin { get; set; }
        public List<MedicamentPrescrit> Medicaments { get; set; } = new List<MedicamentPrescrit>();
    }
}
