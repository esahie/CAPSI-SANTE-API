using CAPSI.Sante.Domain.Models.Firestore;
using Google.Api;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace CAPSI.Sante.API.Hubs
{
    // Hub SignalR pour les notifications en temps réel
    public class NotificationsHub : Hub
    {
        private readonly ILogger<NotificationsHub> _logger;
        private readonly FirestoreDb _firestoreDb;
        private static readonly ConcurrentDictionary<string, string> _userConnections = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, List<FirestoreChangeListener>> _userListeners = new ConcurrentDictionary<string, List<FirestoreChangeListener>>();

        public NotificationsHub(IOptions<FirestoreSettings> settings, ILogger<NotificationsHub> logger)
        {
            _logger = logger;
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", settings.Value.CredentialsPath);
            _firestoreDb = FirestoreDb.Create(settings.Value.ProjectId);
        }

        // Méthode appelée lorsqu'un client se connecte au hub
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            _logger.LogInformation($"Client connecté: {Context.ConnectionId}");
        }

        // Méthode appelée lorsqu'un client se déconnecte du hub
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (_userConnections.TryRemove(Context.ConnectionId, out string userId))
            {
                // Arrêter les listeners pour cet utilisateur
                if (_userListeners.TryGetValue(userId, out List<FirestoreChangeListener> listeners))
                {
                    foreach (var listener in listeners)
                    {
                        listener.StopAsync();
                    }
                    _userListeners.TryRemove(userId, out _);
                }
            }

            await base.OnDisconnectedAsync(exception);
            _logger.LogInformation($"Client déconnecté: {Context.ConnectionId}");
        }

        // S'abonner aux notifications
        public async Task AbonnerNotifications(string userId)
        {
            _userConnections[Context.ConnectionId] = userId;

            if (!_userListeners.TryGetValue(userId, out List<FirestoreChangeListener> listeners))
            {
                listeners = new List<FirestoreChangeListener>();
                _userListeners[userId] = listeners;
            }

            // Écouter les nouvelles notifications
            Query notificationsQuery = _firestoreDb.Collection("notifications")
                .WhereEqualTo("userId", userId)
                .WhereEqualTo("statut", "NonLu");

            FirestoreChangeListener notificationsListener = notificationsQuery.Listen(async snapshot =>
            {
                List<object> notifications = new List<object>();
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists)
                    {
                        notifications.Add(document.ConvertTo<Notification>());
                    }
                }

                await Clients.Client(Context.ConnectionId).SendAsync("NouvellesNotifications", notifications);
            });

            listeners.Add(notificationsListener);

            await Task.CompletedTask;
        }

        // S'abonner aux rendez-vous
        public async Task AbonnerRendezVous(string userId, string userType)
        {
            _userConnections[Context.ConnectionId] = userId;

            if (!_userListeners.TryGetValue(userId, out List<FirestoreChangeListener> listeners))
            {
                listeners = new List<FirestoreChangeListener>();
                _userListeners[userId] = listeners;
            }

            // Définir le champ approprié selon le type d'utilisateur
            string fieldName = userType.ToLower() == "medecin" ? "medecinId" : "patientId";

            // Écouter les changements de rendez-vous
            Query rendezVousQuery = _firestoreDb.Collection("rendez_vous")
                .WhereEqualTo(fieldName, userId);

            FirestoreChangeListener rendezVousListener = rendezVousQuery.Listen(async snapshot =>
            {
                List<object> rendezVous = new List<object>();
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists)
                    {
                        rendezVous.Add(document.ConvertTo<RendezVous>());
                    }
                }

                await Clients.Client(Context.ConnectionId).SendAsync("MisesAJourRendezVous", rendezVous);
            });

            listeners.Add(rendezVousListener);

            await Task.CompletedTask;
        }

        // S'abonner aux messages d'une conversation
        public async Task AbonnerConversation(string conversationId)
        {
            string userId = _userConnections[Context.ConnectionId];

            if (!_userListeners.TryGetValue(userId, out List<FirestoreChangeListener> listeners))
            {
                listeners = new List<FirestoreChangeListener>();
                _userListeners[userId] = listeners;
            }

            // Écouter les nouveaux messages
            Query messagesQuery = _firestoreDb.Collection("messages_temps_reel")
                .WhereEqualTo("conversationId", conversationId)
                .OrderBy("dateEnvoi");

            FirestoreChangeListener messagesListener = messagesQuery.Listen(async snapshot =>
            {
                List<object> messages = new List<object>();
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Exists)
                    {
                        messages.Add(document.ConvertTo<MessageTempsReel>());
                    }
                }

                await Clients.Client(Context.ConnectionId).SendAsync("NouveauxMessages", conversationId, messages);
            });

            listeners.Add(messagesListener);

            await Task.CompletedTask;
        }
    }
}
