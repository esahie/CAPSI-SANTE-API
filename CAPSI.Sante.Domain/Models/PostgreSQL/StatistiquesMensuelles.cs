using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.PostgreSQL
{
    public class StatistiquesMensuelles
    {
        public DateTime Mois { get; set; }
        public int NombreConsultations { get; set; }
        public decimal TauxOccupation { get; set; }
        public decimal TauxAnnulation { get; set; }
    }
}
