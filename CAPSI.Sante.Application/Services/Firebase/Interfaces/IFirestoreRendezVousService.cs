using CAPSI.Sante.Domain.Models.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.Firebase.Interfaces
{
    /// <summary>
    /// Interface pour le service de gestion des rendez-vous dans Firebase
    /// </summary>
    public interface IFirestoreRendezVousService
    {
        /// <summary>
        /// Crée un nouveau rendez-vous avec notifications automatiques
        /// </summary>
        /// <param name="rdv">Le rendez-vous à créer</param>
        /// <returns>L'ID du rendez-vous créé</returns>
        Task<string> CreerRendezVousAsync(RendezVous rdv);

        /// <summary>
        /// Récupère un rendez-vous par son ID
        /// </summary>
        /// <param name="id">L'ID du rendez-vous</param>
        /// <returns>Le rendez-vous ou null s'il n'existe pas</returns>
        Task<RendezVous> GetRendezVousAsync(string id);

        /// <summary>
        /// Récupère tous les rendez-vous d'un patient
        /// </summary>
        /// <param name="patientId">L'ID du patient</param>
        /// <returns>La liste des rendez-vous du patient</returns>
        Task<List<RendezVous>> GetRendezVousPatientAsync(string patientId);

        /// <summary>
        /// Récupère tous les rendez-vous d'un médecin
        /// </summary>
        /// <param name="medecinId">L'ID du médecin</param>
        /// <returns>La liste des rendez-vous du médecin</returns>
        Task<List<RendezVous>> GetRendezVousMedecinAsync(string medecinId);

        /// <summary>
        /// Met à jour le statut d'un rendez-vous
        /// </summary>
        /// <param name="rdvId">L'ID du rendez-vous</param>
        /// <param name="nouveauStatut">Le nouveau statut</param>
        Task UpdateStatutAsync(string rdvId, string nouveauStatut);

        /// <summary>
        /// Annule un rendez-vous et envoie des notifications
        /// </summary>
        /// <param name="rdvId">L'ID du rendez-vous</param>
        /// <param name="motif">Le motif d'annulation (optionnel)</param>
        Task AnnulerRendezVousAsync(string rdvId, string motif = null);

        /// <summary>
        /// Récupère tous les rendez-vous
        /// </summary>
        /// <returns>La liste de tous les rendez-vous</returns>
        Task<List<RendezVous>> GetAllRendezVousAsync();

        /// <summary>
        /// Vérifie si un type de consultation est utilisé dans des rendez-vous
        /// </summary>
        /// <param name="typeConsultation">Le type de consultation</param>
        /// <returns>True si le type est utilisé, sinon False</returns>
        Task<bool> IsServiceUsedAsync(string typeConsultation);

        /// <summary>
        /// Vérifie si un service (par ID SQL) est utilisé dans des rendez-vous
        /// </summary>
        /// <param name="serviceId">L'ID du service</param>
        /// <returns>True si le service est utilisé, sinon False</returns>
        Task<bool> IsServiceIdUsedAsync(string serviceId);

        /// <summary>
        /// Récupère les rendez-vous d'un médecin à partir d'une date spécifique
        /// </summary>
        /// <param name="medecinId">L'ID du médecin</param>
        /// <param name="dateDebut">La date de début pour filtrer les rendez-vous</param>
        /// <returns>La liste des rendez-vous du médecin à partir de la date spécifiée</returns>
        Task<IEnumerable<RendezVous>> GetByMedecinIdAndDateAsync(Guid medecinId, DateTime dateDebut);

    }
}
