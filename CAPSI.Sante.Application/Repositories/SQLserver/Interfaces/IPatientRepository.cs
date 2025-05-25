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
    }
}
