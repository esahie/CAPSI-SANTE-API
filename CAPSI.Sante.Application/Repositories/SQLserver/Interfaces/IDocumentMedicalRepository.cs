using CAPSI.Sante.Domain.Models.SQLserver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Repositories.SQLserver.Interfaces
{
    public interface IDocumentMedicalRepository : IBaseRepository<DocumentMedical>
    {
        // Méthodes héritées de IBaseRepository<T>
        Task<DocumentMedical> GetByIdAsync(Guid id);
        Task<IEnumerable<DocumentMedical>> GetAllAsync();
        Task<DocumentMedical> AddAsync(DocumentMedical entity);

        Task<DocumentMedical> UpdateAsync(DocumentMedical entity);
        //Task UpdateAsync(DocumentMedical entity);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<DocumentMedical>> GetByDossierIdAsync(Guid dossierId);
        Task<IEnumerable<DocumentMedical>> GetByPatientIdAsync(Guid patientId);
        Task<IEnumerable<DocumentMedical>> GetByTypeAsync(Guid dossierId, string type);

        // Méthodes utilitaires
        Task<bool> ExistsAsync(Guid id);
        Task<int> GetDocumentCountByDossierAsync(Guid dossierId);
        Task<IEnumerable<DocumentMedical>> GetRecentDocumentsAsync(Guid dossierId, int count = 5);

    }

}
