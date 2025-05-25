using CAPSI.Sante.Domain.Models.SQLserver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Repositories.SQLserver.Interfaces
{
    public interface IPrescriptionRepository
    {
        Task<Prescription> AddAsync(Prescription prescription);
        Task<Prescription> GetByIdAsync(Guid id);
        Task<IEnumerable<Prescription>> GetByDossierIdAsync(Guid dossierId);
        Task<IEnumerable<Prescription>> GetByMedecinIdAsync(Guid medecinId);
        Task<Prescription> UpdateAsync(Prescription prescription);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
