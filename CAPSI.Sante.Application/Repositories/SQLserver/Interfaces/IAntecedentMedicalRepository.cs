using CAPSI.Sante.Domain.Models.SQLserver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Repositories.SQLserver.Interfaces
{
    public interface IAntecedentMedicalRepository
    {
        Task<AntecedentMedical> AddAsync(AntecedentMedical antecedent);
        Task<AntecedentMedical> GetByIdAsync(Guid id);
        Task<IEnumerable<AntecedentMedical>> GetByDossierIdAsync(Guid dossierId);
        Task<IEnumerable<AntecedentMedical>> GetByTypeAsync(Guid dossierId, string type);
        Task<AntecedentMedical> UpdateAsync(AntecedentMedical antecedent);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
