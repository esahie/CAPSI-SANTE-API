using CAPSI.Sante.Application.DTOs.AntecedentMedical;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.SQLserver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.SQLserver.Interfaces
{
    public interface IAntecedentMedicalService
    {
        Task<ApiResponse<AntecedentMedical>> CreateAntecedentAsync(CreateAntecedentMedicalDto dto);
        Task<ApiResponse<AntecedentMedical>> GetAntecedentByIdAsync(Guid id);
        Task<ApiResponse<List<AntecedentMedical>>> GetAntecedentsByDossierAsync(Guid dossierId, string type = null);
        Task<ApiResponse<AntecedentMedical>> UpdateAntecedentAsync(UpdateAntecedentMedicalDto dto);
        Task<ApiResponse<bool>> DeleteAntecedentAsync(Guid id);
    }
}
