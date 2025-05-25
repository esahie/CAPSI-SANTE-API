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
    // Service de gestion des messages en temps réel
    public class MessageService : FirestoreService
    {
        private const string COLLECTION_NAME = "messages_temps_reel";

        public MessageService(IOptions<FirestoreSettings> settings) : base(settings) { }

        // Envoyer un message
        public async Task<string> EnvoyerMessageAsync(MessageTempsReel message)
        {
            DocumentReference docRef = _firestoreDb.Collection(COLLECTION_NAME).Document();
            message.Id = docRef.Id;
            message.DateEnvoi = DateTime.UtcNow;
            message.Statut = "Envoyé";

            await docRef.SetAsync(message);
            return message.Id;
        }

        // Récupérer les messages d'une conversation
        public async Task<List<MessageTempsReel>> GetMessagesConversationAsync(string conversationId)
        {
            Query query = _firestoreDb.Collection(COLLECTION_NAME)
                .WhereEqualTo("conversationId", conversationId)
                .OrderBy("dateEnvoi");

            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            return querySnapshot.Documents
                .Select(doc => doc.ConvertTo<MessageTempsReel>())
                .ToList();
        }

        // Marquer un message comme reçu
        public async Task MarquerCommeRecuAsync(string messageId)
        {
            DocumentReference docRef = _firestoreDb.Collection(COLLECTION_NAME).Document(messageId);

            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "statut", "Reçu" }
            };

            await docRef.UpdateAsync(updates);
        }

        // Marquer un message comme lu
        public async Task MarquerCommeLuAsync(string messageId)
        {
            DocumentReference docRef = _firestoreDb.Collection(COLLECTION_NAME).Document(messageId);

            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "statut", "Lu" }
            };

            await docRef.UpdateAsync(updates);
        }

        // Marquer tous les messages d'une conversation comme lus
        public async Task MarquerTousCommeLuAsync(string conversationId, string destinataireId)
        {
            var batch = _firestoreDb.StartBatch();

            Query query = _firestoreDb.Collection(COLLECTION_NAME)
                .WhereEqualTo("conversationId", conversationId)
                .WhereEqualTo("destinataireId", destinataireId)
                .WhereNotEqualTo("statut", "Lu");

            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            foreach (DocumentSnapshot doc in querySnapshot.Documents)
            {
                batch.Update(doc.Reference, new Dictionary<string, object>
                {
                    { "statut", "Lu" }
                });
            }

            await batch.CommitAsync();
        }
    }
}
