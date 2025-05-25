using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.Firestore
{
    // Nouveau modèle pour stocker les références aux services médicaux
    [FirestoreData]
    public class ServiceMedicalReference
    {
        [FirestoreProperty("id")]
        public string Id { get; set; }  // ID SQL du service

        [FirestoreProperty("code")]
        public string Code { get; set; }

        [FirestoreProperty("nom")]
        public string Nom { get; set; }
    }
}
