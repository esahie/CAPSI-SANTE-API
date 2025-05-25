using CAPSI.Sante.Application.Services.Firebase.Interfaces;
using CAPSI.Sante.Domain.Models.Firestore;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Infrastructure.Firebase.Services
{
    /// <summary>
    /// Service de gestion des rendez-vous dans Firebase
    /// </summary>
    public class FirestoreRendezVous : FirestoreService, IFirestoreRendezVousService
    {
        private const string COLLECTION_NAME = "rendez_vous";
        private const string NOTIFICATIONS_COLLECTION = "notifications";

        public FirestoreRendezVous(IOptions<FirestoreSettings> settings) : base(settings) { }

        /// <inheritdoc/>
        public async Task<string> CreerRendezVousAsync(RendezVous rdv)
        {
            var batch = _firestoreDb.StartBatch();

            // Générer un nouvel ID pour le document
            DocumentReference rdvRef = _firestoreDb.Collection(COLLECTION_NAME).Document();
            rdv.Id = rdvRef.Id;
            rdv.CreatedAt = DateTime.UtcNow;
            rdv.UpdatedAt = DateTime.UtcNow;

            // Ajouter le document RDV
            batch.Set(rdvRef, rdv);

            // Créer notification pour le patient
            DocumentReference notifPatientRef = _firestoreDb.Collection(NOTIFICATIONS_COLLECTION).Document();
            var notifPatient = new Notification
            {
                Id = notifPatientRef.Id,
                UserId = rdv.PatientId,
                Type = "RDV",
                Message = $"Nouveau rendez-vous le {rdv.DateHeure.ToLocalTime():dd/MM/yyyy à HH:mm}",
                Statut = "NonLu",
                DateCreation = DateTime.UtcNow
            };
            batch.Set(notifPatientRef, notifPatient);

            // Créer notification pour le médecin
            DocumentReference notifMedecinRef = _firestoreDb.Collection(NOTIFICATIONS_COLLECTION).Document();
            var notifMedecin = new Notification
            {
                Id = notifMedecinRef.Id,
                UserId = rdv.MedecinId,
                Type = "RDV",
                Message = $"Nouvelle consultation planifiée le {rdv.DateHeure.ToLocalTime():dd/MM/yyyy à HH:mm}",
                Statut = "NonLu",
                DateCreation = DateTime.UtcNow
            };
            batch.Set(notifMedecinRef, notifMedecin);

            // Exécuter toutes les opérations en une seule transaction
            await batch.CommitAsync();
            return rdv.Id;
        }

        /// <inheritdoc/>
        public async Task<RendezVous> GetRendezVousAsync(string id)
        {
            DocumentReference docRef = _firestoreDb.Collection(COLLECTION_NAME).Document(id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<RendezVous>();
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<List<RendezVous>> GetRendezVousPatientAsync(string patientId)
        {
            Query query = _firestoreDb.Collection(COLLECTION_NAME)
                .WhereEqualTo("patientId", patientId);

            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            return querySnapshot.Documents
                .Select(doc => doc.ConvertTo<RendezVous>())
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<List<RendezVous>> GetRendezVousMedecinAsync(string medecinId)
        {
            Query query = _firestoreDb.Collection(COLLECTION_NAME)
                .WhereEqualTo("medecinId", medecinId);

            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            return querySnapshot.Documents
                .Select(doc => doc.ConvertTo<RendezVous>())
                .ToList();
        }

        /// <inheritdoc/>
        public async Task UpdateStatutAsync(string rdvId, string nouveauStatut)
        {
            DocumentReference docRef = _firestoreDb.Collection(COLLECTION_NAME).Document(rdvId);

            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "statut", nouveauStatut },
                { "updatedAt", DateTime.UtcNow }
            };

            await docRef.UpdateAsync(updates);
        }

        /// <inheritdoc/>
        public async Task AnnulerRendezVousAsync(string rdvId, string motif = null)
        {
            var batch = _firestoreDb.StartBatch();

            // Mettre à jour le RDV
            DocumentReference rdvRef = _firestoreDb.Collection(COLLECTION_NAME).Document(rdvId);
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "statut", "Annulé" },
                { "updatedAt", DateTime.UtcNow }
            };

            if (!string.IsNullOrEmpty(motif))
            {
                updates.Add("notes", motif);
            }

            batch.Update(rdvRef, updates);

            // Récupérer les informations du rendez-vous
            DocumentSnapshot rdvSnapshot = await rdvRef.GetSnapshotAsync();
            if (rdvSnapshot.Exists)
            {
                var rdv = rdvSnapshot.ConvertTo<RendezVous>();

                // Notification pour le patient
                DocumentReference notifPatientRef = _firestoreDb.Collection(NOTIFICATIONS_COLLECTION).Document();
                var notifPatient = new Notification
                {
                    Id = notifPatientRef.Id,
                    UserId = rdv.PatientId,
                    Type = "RDV",
                    Message = $"Votre rendez-vous du {rdv.DateHeure.ToLocalTime():dd/MM/yyyy à HH:mm} a été annulé",
                    Statut = "NonLu",
                    DateCreation = DateTime.UtcNow
                };
                batch.Set(notifPatientRef, notifPatient);

                // Notification pour le médecin
                DocumentReference notifMedecinRef = _firestoreDb.Collection(NOTIFICATIONS_COLLECTION).Document();
                var notifMedecin = new Notification
                {
                    Id = notifMedecinRef.Id,
                    UserId = rdv.MedecinId,
                    Type = "RDV",
                    Message = $"Le rendez-vous du {rdv.DateHeure.ToLocalTime():dd/MM/yyyy à HH:mm} a été annulé",
                    Statut = "NonLu",
                    DateCreation = DateTime.UtcNow
                };
                batch.Set(notifMedecinRef, notifMedecin);
            }

            await batch.CommitAsync();
        }

        /// <inheritdoc/>
        public async Task<List<RendezVous>> GetAllRendezVousAsync()
        {
            Query query = _firestoreDb.Collection(COLLECTION_NAME);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            return querySnapshot.Documents
                .Select(doc => doc.ConvertTo<RendezVous>())
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<bool> IsServiceUsedAsync(string typeConsultation)
        {
            if (string.IsNullOrEmpty(typeConsultation))
                return false;

            Query query = _firestoreDb.Collection(COLLECTION_NAME)
                .WhereEqualTo("typeConsultation", typeConsultation);

            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            return querySnapshot.Documents.Count > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> IsServiceIdUsedAsync(string serviceId)
        {
            if (string.IsNullOrEmpty(serviceId))
                return false;

            // Si vous stockez l'ID SQL du service dans vos documents Firebase
            Query query = _firestoreDb.Collection(COLLECTION_NAME)
                .WhereEqualTo("serviceMedicalId", serviceId);

            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            return querySnapshot.Documents.Count > 0;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<RendezVous>> GetByMedecinIdAndDateAsync(Guid medecinId, DateTime dateDebut)
        {
            // Convertir le Guid en string pour la requête Firestore
            string medecinIdString = medecinId.ToString();

            // Créer la requête pour récupérer les rendez-vous du médecin à partir de la date spécifiée
            Query query = _firestoreDb.Collection(COLLECTION_NAME)
                .WhereEqualTo("medecinId", medecinIdString)
                .WhereGreaterThanOrEqualTo("dateHeure", dateDebut);

            // Exécuter la requête
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            // Convertir les résultats en objets RendezVous
            return querySnapshot.Documents
                .Select(doc => doc.ConvertTo<RendezVous>())
                .ToList();
        }
    }
}
