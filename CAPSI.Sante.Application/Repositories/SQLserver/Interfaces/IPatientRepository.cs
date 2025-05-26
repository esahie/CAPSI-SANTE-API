using CAPSI.Sante.Domain.Models.SQLserver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Repositories.SQLserver.Interfaces
{
    public interface IPatientRepository : IBaseRepository<Patient>
    {
        Task<Patient> GetByNumeroAssuranceAsync(string numeroAssurance);
        Task<IEnumerable<Patient>> SearchAsync(string searchTerm);

        Task<bool> DeactivateAsync(Guid id);
        Task<bool> ReactivateAsync(Guid id);
        Task<IEnumerable<Patient>> GetAllAsync(bool includeInactive = false);
        Task<IEnumerable<Patient>> SearchAsync(string searchTerm, bool includeInactive = false);
        // Ajoutez cette méthode pour résoudre l'erreur
        Task<bool> ExistsAsync(Guid id);

        // Ajoutez également cette méthode pour la validation des numéros d'assurance
        Task<bool> IsNumeroAssuranceUnique(string numeroAssurance, Guid? excludeId = null);
        // Nouvelles méthodes pour la gestion des photos
        Task<bool> UpdatePhotoAsync(Guid patientId, string photoUrl);
        Task<bool> DeletePhotoAsync(Guid patientId);
        Task<string> GetPhotoUrlAsync(Guid patientId);

        // NOUVELLES MÉTHODES DE VALIDATION - AJOUTEZ CES LIGNES
        Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null);
        Task<bool> IsTelephoneUniqueAsync(string telephone, Guid? excludeId = null);
        Task<bool> IsUserIdUniqueAsync(Guid? userId, Guid? excludeId = null);
        Task<Patient> GetByEmailAsync(string email);
        Task<Patient> GetByTelephoneAsync(string telephone);
        Task<Patient> GetByUserIdAsync(Guid userId);

        // Nouvelles méthodes pour réactivation
        Task<Patient> FindInactivePatientByEmailAsync(string email);
        Task<Patient> FindInactivePatientByTelephoneAsync(string telephone);
        Task<Patient> FindInactivePatientByUserIdAsync(Guid userId);
        Task<Patient> FindInactivePatientByNumeroAssuranceAsync(string numeroAssurance);
        Task<Patient> FindInactivePatientByNomCompletAsync(string nom, string prenom);
        Task<Guid> CreateDemandeReactivationAsync(Guid patientId, string emailDemandeur, string motifDemande = null);
        Task<bool> ConfirmReactivationByTokenAsync(string token);
    }
}
