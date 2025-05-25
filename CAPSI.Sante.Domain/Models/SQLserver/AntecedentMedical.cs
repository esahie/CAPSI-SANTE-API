using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.SQLserver
{
    public class AntecedentMedical
    {
        public Guid Id { get; set; }
        public Guid DossierId { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime? DateDiagnostic { get; set; }

        // Propriété de navigation (non stockée en base)
        public DossierMedical Dossier { get; set; }
    }
}
