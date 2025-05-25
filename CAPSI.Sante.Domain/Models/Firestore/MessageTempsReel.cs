using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.Firestore
{
    // Modèle de Message Temps Réel
    [FirestoreData]
    public class MessageTempsReel
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty("conversationId")]
        public string ConversationId { get; set; }

        [FirestoreProperty("expediteurId")]
        public string ExpediteurId { get; set; }

        [FirestoreProperty("destinataireId")]
        public string DestinataireId { get; set; }

        [FirestoreProperty("contenu")]
        public string Contenu { get; set; }

        [FirestoreProperty("dateEnvoi")]
        public DateTime DateEnvoi { get; set; }

        [FirestoreProperty("statut")]
        public string Statut { get; set; }
    }
}
