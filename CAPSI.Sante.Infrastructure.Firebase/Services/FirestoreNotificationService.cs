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
    // Service de gestion des notifications
    public class FirestoreNotificationService : FirestoreService
    {
        private const string COLLECTION_NAME = "notifications";

        public FirestoreNotificationService(IOptions<FirestoreSettings> settings) : base(settings) { }

        // Envoyer une notification
        public async Task<string> EnvoyerNotificationAsync(Notification notification)
        {
            DocumentReference docRef = _firestoreDb.Collection(COLLECTION_NAME).Document();
            notification.Id = docRef.Id;
            notification.DateCreation = DateTime.UtcNow;
            notification.Statut = "NonLu";

            await docRef.SetAsync(notification);
            return notification.Id;
        }

        // Récupérer les notifications non lues d'un utilisateur
        public async Task<List<Notification>> GetNotificationsNonLuesAsync(string userId)
        {
            Query query = _firestoreDb.Collection(COLLECTION_NAME)
                .WhereEqualTo("userId", userId)
                .WhereEqualTo("statut", "NonLu");

            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            return querySnapshot.Documents
                .Select(doc => doc.ConvertTo<Notification>())
                .ToList();
        }

        // Récupérer toutes les notifications d'un utilisateur
        public async Task<List<Notification>> GetNotificationsAsync(string userId)
        {
            Query query = _firestoreDb.Collection(COLLECTION_NAME)
                .WhereEqualTo("userId", userId)
                .OrderByDescending("dateCreation");

            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            return querySnapshot.Documents
                .Select(doc => doc.ConvertTo<Notification>())
                .ToList();
        }

        // Marquer une notification comme lue
        public async Task MarquerCommeLueAsync(string notificationId)
        {
            DocumentReference docRef = _firestoreDb.Collection(COLLECTION_NAME).Document(notificationId);

            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "statut", "Lu" },
                { "dateLecture", DateTime.UtcNow }
            };

            await docRef.UpdateAsync(updates);
        }

        // Supprimer une notification
        public async Task SupprimerNotificationAsync(string notificationId)
        {
            DocumentReference docRef = _firestoreDb.Collection(COLLECTION_NAME).Document(notificationId);
            await docRef.DeleteAsync();
        }
    }
}
