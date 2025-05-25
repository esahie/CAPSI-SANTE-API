using CAPSI.Sante.Application.DTOs.DossierMedical;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.SQLserver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.SQLserver.Interfaces
{
    public interface IDossierMedicalService
    {
        Task<ApiResponse<DossierMedical>> CreateDossierAsync(CreateDossierMedicalDto dto);
        Task<ApiResponse<DossierMedical>> GetDossierByIdAsync(Guid id);
        Task<ApiResponse<DossierMedical>> GetDossierByPatientIdAsync(Guid patientId);
        Task<ApiResponse<List<DossierMedical>>> GetAllDossiersAsync();
        Task<ApiResponse<DossierMedical>> UpdateDossierAsync(UpdateDossierMedicalDto dto);
        Task<ApiResponse<bool>> DeleteDossierAsync(Guid id);
    }
}
