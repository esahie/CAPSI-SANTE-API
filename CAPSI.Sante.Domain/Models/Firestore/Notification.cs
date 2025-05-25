using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.Firestore
{
    // Modèle de Notification
    [FirestoreData]
    public class Notification
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty("userId")]
        public string UserId { get; set; }

        [FirestoreProperty("type")]
        public string Type { get; set; }

        [FirestoreProperty("message")]
        public string Message { get; set; }

        [FirestoreProperty("statut")]
        public string Statut { get; set; }

        [FirestoreProperty("dateCreation")]
        public DateTime DateCreation { get; set; }

        [FirestoreProperty("dateLecture")]
        public DateTime? DateLecture { get; set; }

        [FirestoreProperty("metadata")]
        public Dictionary<string, object> Metadata { get; set; }

    }
}
