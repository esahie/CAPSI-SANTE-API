using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.PostgreSQL
{
    public class StatistiquesJournalieres
    {
        public DateTime Date { get; set; }
        public int NombreConsultations { get; set; }
        public int DureeMoyenneConsultation { get; set; }
        public decimal TauxOccupation { get; set; }
        public decimal TauxPonctualite { get; set; }
        public int NombreAnnulations { get; set; }
    }
}
