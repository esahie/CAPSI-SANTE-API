using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.Firestore
{
    // Modèle de Disponibilité
    [FirestoreData]
    public class Disponibilite
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty("medecinId")]
        public string MedecinId { get; set; }

        [FirestoreProperty("date")]
        public DateTime Date { get; set; }

        [FirestoreProperty("creneaux")]
        public List<Creneau> Creneaux { get; set; }
    }
}
