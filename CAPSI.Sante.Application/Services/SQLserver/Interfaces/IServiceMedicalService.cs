using CAPSI.Sante.Application.DTOs.ServiceMedical;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.SQLserver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.SQLserver.Interfaces
{
    public interface IServiceMedicalService
    {
        Task<ApiResponse<ServiceMedical>> CreateServiceAsync(CreateServiceMedicalDto dto);
        Task<ApiResponse<ServiceMedical>> GetServiceByIdAsync(Guid id);
        Task<ApiResponse<ServiceMedical>> GetServiceByCodeAsync(string code);
        Task<ApiResponse<List<ServiceMedical>>> GetAllServicesAsync(bool includeInactive = false);
        Task<ApiResponse<List<ServiceMedical>>> GetServicesByMedecinAsync(Guid medecinId, bool includeInactive = false);
        Task<ApiResponse<ServiceMedical>> UpdateServiceAsync(UpdateServiceMedicalDto dto);
        Task<ApiResponse<ServiceMedical>> ActivateServiceAsync(Guid id);
        Task<ApiResponse<ServiceMedical>> DeactivateServiceAsync(Guid id);
        Task<ApiResponse<bool>> DeleteServiceAsync(Guid id);
    }
}