using CAPSI.Sante.Domain.Models.SQLserver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Repositories.SQLserver.Interfaces
{
    public interface IMedecinRepository : IBaseRepository<Medecin>
    {
        Task<Medecin> AddAsync(Medecin medecin);
        Task<Medecin> GetByIdAsync(Guid id);
       
        Task<IEnumerable<Medecin>> GetAllAsync();
        Task<Medecin> UpdateAsync(Medecin medecin);
        Task<Medecin> UpdateMedecinInfoAsync(Medecin medecin);
        Task UpdateServicesAsync(Guid medecinId, List<Guid> serviceIds);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<Medecin>> GetBySpecialiteAsync(string specialite);
        Task<IEnumerable<Medecin>> GetByServiceAsync(Guid serviceId);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> IsLicenceUnique(string numeroLicence, Guid? excludeId = null);
        Task<bool> UpdatePhotoAsync(Guid medecinId, string photoUrl, string photoNom, string photoType, long photoTaille);
        Task<bool> RemovePhotoAsync(Guid medecinId);
        Task<Medecin> GetByLicenceAsync(string numeroLicence);
        Task<Medecin> GetByLicenceAsync(string numeroLicence, IDbConnection existingConnection = null);
        Task<Medecin> AddAsync(Medecin medecin, IDbConnection existingConnection = null);

        Task<Medecin> CreateWithLicenceCheckAsync(Medecin medecin);

    }
}
