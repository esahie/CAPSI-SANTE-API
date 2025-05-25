using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.PostgreSQL
{
    public class StatistiquesGlobales
    {
        public int TotalConsultations { get; set; }
        public int NombreMedecinsActifs { get; set; }
        public int NombrePatientsUniques { get; set; }
        public decimal TauxOccupationMoyen { get; set; }
        public decimal TauxAnnulation { get; set; }
        public List<StatistiquesMensuelles> StatistiquesParMois { get; set; }
    }
}
