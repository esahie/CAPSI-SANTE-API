using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.PostgreSQL
{
    public class TendanceSante
    {
        public DateTime PeriodeDebut { get; set; }
        public DateTime PeriodeFin { get; set; }
        public string TypeTendance { get; set; }
        public string Description { get; set; }
        public decimal ValeurNumerique { get; set; }
    }
}
