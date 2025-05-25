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
    // Service de gestion des disponibilités
    public class DisponibiliteService : FirestoreService
    {
        private const string COLLECTION_NAME = "disponibilites_medecins";

        public DisponibiliteService(IOptions<FirestoreSettings> settings) : base(settings) { }

        // Créer ou mettre à jour les disponibilités d'un médecin
        public async Task<string> SetDisponibilitesAsync(Disponibilite disponibilite)
        {
            // Vérifier si une disponibilité existe déjà pour cette date et ce médecin
            Query query = _firestoreDb.Collection(COLLECTION_NAME)
                .WhereEqualTo("medecinId", disponibilite.MedecinId)
                .WhereEqualTo("date", disponibilite.Date.Date);

            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            DocumentReference docRef;

            if (querySnapshot.Documents.Count > 0)
            {
                // Mise à jour du document existant
                docRef = querySnapshot.Documents[0].Reference;
                disponibilite.Id = docRef.Id;
                await docRef.SetAsync(disponibilite);
            }
            else
            {
                // Création d'un nouveau document
                docRef = _firestoreDb.Collection(COLLECTION_NAME).Document();
                disponibilite.Id = docRef.Id;
                await docRef.SetAsync(disponibilite);
            }

            return disponibilite.Id;
        }

        // Obtenir les créneaux disponibles pour un médecin
        public async Task<List<Disponibilite>> GetCreneauxDisponiblesAsync(
            string medecinId, DateTime dateDebut, DateTime dateFin)
        {
            Query query = _firestoreDb.Collection(COLLECTION_NAME)
                .WhereEqualTo("medecinId", medecinId)
                .WhereGreaterThanOrEqualTo("date", dateDebut.Date)
                .WhereLessThanOrEqualTo("date", dateFin.Date);

            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            return querySnapshot.Documents
                .Select(doc => doc.ConvertTo<Disponibilite>())
                .ToList();
        }

        // Obtenir les disponibilités d'un médecin pour une date spécifique
        public async Task<Disponibilite> GetDisponibilitesDateAsync(string medecinId, DateTime date)
        {
            Query query = _firestoreDb.Collection(COLLECTION_NAME)
                .WhereEqualTo("medecinId", medecinId)
                .WhereEqualTo("date", date.Date);

            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            return querySnapshot.Documents.FirstOrDefault()?.ConvertTo<Disponibilite>();
        }

        // Mettre à jour un créneau spécifique
        public async Task UpdateCreneauAsync(string disponibiliteId, Creneau creneau)
        {
            DocumentReference docRef = _firestoreDb.Collection(COLLECTION_NAME).Document(disponibiliteId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                var disponibilite = snapshot.ConvertTo<Disponibilite>();

                // Trouver et mettre à jour le créneau
                var creneauExistant = disponibilite.Creneaux
                    .FirstOrDefault(c => c.Debut == creneau.Debut && c.Fin == creneau.Fin);

                if (creneauExistant != null)
                {
                    creneauExistant.Disponible = creneau.Disponible;
                    await docRef.SetAsync(disponibilite);
                }
            }
        }
    }

}
