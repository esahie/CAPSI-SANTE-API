using CAPSI.Sante.Domain.Models.SQLserver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Repositories.SQLserver.Interfaces
{
    public interface IServiceMedicalRepository
    {
        // CREATE
        Task<ServiceMedical> AddAsync(ServiceMedical entity);

        // READ
        Task<ServiceMedical> GetByIdAsync(Guid id);
        Task<ServiceMedical> GetByCodeAsync(string code);
        Task<IEnumerable<ServiceMedical>> GetAllAsync(bool includeInactive = false);
        Task<IEnumerable<ServiceMedical>> GetByMedecinAsync(Guid medecinId, bool includeInactive = false);

        // UPDATE
        Task<ServiceMedical> UpdateAsync(ServiceMedical entity);

        // ACTIVATE/DEACTIVATE
        Task<ServiceMedical> ActivateAsync(Guid id);
        Task<ServiceMedical> DeactivateAsync(Guid id);

        // DELETE
        Task DeleteAsync(Guid id);

        // UTILITY
        Task<bool> IsUsedInAppointmentsAsync(Guid serviceId);
        Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null);
    }
}