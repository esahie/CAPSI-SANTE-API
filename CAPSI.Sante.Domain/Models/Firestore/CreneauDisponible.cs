using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.Firestore
{
    public class CreneauDisponible
    {
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int DureeMinutes { get; set; }
        public bool EstDisponible { get; set; }
    }
}
