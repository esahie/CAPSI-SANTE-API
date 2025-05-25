using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.Firestore
{
    // Modèle de Créneau
    [FirestoreData]
    public class Creneau
    {
        [FirestoreProperty("debut")]
        public DateTime Debut { get; set; }

        [FirestoreProperty("fin")]
        public DateTime Fin { get; set; }

        [FirestoreProperty("disponible")]
        public bool Disponible { get; set; }
    }
}
