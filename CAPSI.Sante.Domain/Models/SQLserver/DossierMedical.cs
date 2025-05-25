using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.SQLserver
{
    public class DossierMedical
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DerniereMiseAJour { get; set; }

        // Propriétés de navigation (non stockées en base)
        public Patient Patient { get; set; }
        public List<AntecedentMedical> Antecedents { get; set; } = new List<AntecedentMedical>();
        public List<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public List<DocumentMedical> Documents { get; set; } = new List<DocumentMedical>();
    }
}
