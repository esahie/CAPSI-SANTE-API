using CAPSI.Sante.Domain.Models.SQLserver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Repositories.SQLserver.Interfaces
{
    public interface IDossierMedicalRepository : IBaseRepository<DossierMedical>
    {
        Task<DossierMedical> GetByPatientIdAsync(Guid patientId);
        Task<bool> ExistsAsync(Guid id);

    }
}
