using CAPSI.Sante.Application.DTOs.Patient;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.SQLserver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.SQLserver.Interfaces
{
    public interface IPatientService
    {
        Task<ApiResponse<Patient>> CreatePatientAsync(CreatePatientDto dto);
        Task<ApiResponse<Patient>> GetPatientByIdAsync(Guid id);
        Task<ApiResponse<List<Patient>>> GetPatientsAsync(string searchTerm = null, int page = 1, int pageSize = 10);
        Task<ApiResponse<Patient>> UpdatePatientAsync(UpdatePatientDto dto);
        Task<ApiResponse<bool>> DeletePatientAsync(Guid id);
        Task<ApiResponse<Patient>> GetByNumeroAssuranceAsync(string numeroAssurance);
        Task<ApiResponse<List<Patient>>> SearchPatientsAsync(string searchTerm);

        Task<ApiResponse<bool>> DeactivatePatientAsync(Guid id);
        Task<ApiResponse<bool>> ReactivatePatientAsync(Guid id);
        Task<ApiResponse<List<Patient>>> GetPatientsAsync(string searchTerm = null, int page = 1, int pageSize = 10, bool includeInactive = false);

    }
}
