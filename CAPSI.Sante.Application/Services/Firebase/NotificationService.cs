using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using CAPSI.Sante.Domain.Enums;
using CAPSI.Sante.Domain.Models;
using CAPSI.Sante.Domain.Models.Firestore;
using CAPSI.Sante.Application.Services.SQLserver.Interfaces;

namespace CAPSI.Sante.Application.Services.Firebase
{
    public class NotificationService : INotificationService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
        FirestoreDb firestoreDb,
        ILogger<NotificationService> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }
        public async Task EnvoyerNotificationRendezVousAsync(
            RendezVous rdv,
            NotificationType type,
            string message = null)
        {
            try
            {
                var notifCollection = _firestoreDb.Collection("notifications");

                // Notification pour le patient
                await notifCollection.AddAsync(new
                {
                    userId = rdv.PatientId.ToString(),
                    type = type.ToString(),
                    message = message ?? GenererMessageNotification(type, rdv, "patient"),
                    statut = "NonLu",
                    dateCreation = DateTime.UtcNow
                });

                // Notification pour le médecin
                await notifCollection.AddAsync(new
                {
                    userId = rdv.MedecinId.ToString(),
                    type = type.ToString(),
                    message = message ?? GenererMessageNotification(type, rdv, "medecin"),
                    statut = "NonLu",
                    dateCreation = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi des notifications");
                throw;
            }
        }

        private string GenererMessageNotification(NotificationType type, RendezVous rdv, string destinataire)
        {
            return type switch
            {
                NotificationType.NouveauRendezVous => destinataire == "patient"
                    ? $"Votre rendez-vous est confirmé pour le {rdv.DateHeure:dd/MM/yyyy à HH:mm}"
                    : $"Nouvelle consultation planifiée le {rdv.DateHeure:dd/MM/yyyy à HH:mm}",

                NotificationType.AnnulationRendezVous => destinataire == "patient"
                    ? $"Votre rendez-vous du {rdv.DateHeure:dd/MM/yyyy à HH:mm} a été annulé"
                    : $"La consultation du {rdv.DateHeure:dd/MM/yyyy à HH:mm} a été annulée",

                NotificationType.RappelRendezVous => destinataire == "patient"
                    ? $"Rappel : vous avez rendez-vous demain à {rdv.DateHeure:HH:mm}"
                    : $"Rappel : consultation prévue demain à {rdv.DateHeure:HH:mm}",

                _ => "Notification concernant votre rendez-vous"
            };
        }
    }
}
