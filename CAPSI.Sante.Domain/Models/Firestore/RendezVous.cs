using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Domain.Models.Firestore
{
    // Modèle de RendezVous
    [FirestoreData]
    public class RendezVous
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty("patientId")]
        public string PatientId { get; set; }

        [FirestoreProperty("medecinId")]
        public string MedecinId { get; set; }

        [FirestoreProperty("dateHeure")]
        public DateTime DateHeure { get; set; }

        [FirestoreProperty("statut")]
        public string Statut { get; set; }

        [FirestoreProperty("typeConsultation")]
        public string TypeConsultation { get; set; }

        // Nouveau champ pour stocker l'ID SQL du service médical
        [FirestoreProperty("serviceMedicalId")]
        public string ServiceMedicalId { get; set; }

        // Nouveau champ pour stocker le code du service médical (pour faciliter les recherches)
        [FirestoreProperty("serviceMedicalCode")]
        public string ServiceMedicalCode { get; set; }


        [FirestoreProperty("dureeMinutes")]
        public int DureeMinutes { get; set; }

        [FirestoreProperty("notes")]
        public string Notes { get; set; }

        // Nouveau champ pour stocker les services associés (si plusieurs services par rendez-vous)
        [FirestoreProperty("servicesMedicaux")]
        public List<ServiceMedicalReference> ServicesMedicaux { get; set; }


        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
