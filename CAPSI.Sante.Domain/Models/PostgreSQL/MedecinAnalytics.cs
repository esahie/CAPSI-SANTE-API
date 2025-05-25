using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.PostgreSQL
{
    public class MedecinAnalytics
    {
        public int TotalConsultations { get; set; }
        public decimal TauxSatisfactionMoyen { get; set; }
        public decimal TauxPonctualite { get; set; }
        public int DureeMoyenneConsultation { get; set; }
        public List<StatistiquesJournalieres> StatistiquesParJour { get; set; }
    }
}
