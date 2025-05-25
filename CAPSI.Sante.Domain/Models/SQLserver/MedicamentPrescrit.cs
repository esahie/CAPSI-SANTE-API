using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.SQLserver
{
    public class MedicamentPrescrit
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public string NomMedicament { get; set; }
        public string Posologie { get; set; }
        public int? Duree { get; set; }
        public string Instructions { get; set; }

        // Propriété de navigation (non stockée en base)
        public Prescription Prescription { get; set; }
    }
}
