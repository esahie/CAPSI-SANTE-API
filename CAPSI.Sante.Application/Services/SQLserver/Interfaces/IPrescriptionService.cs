using CAPSI.Sante.Application.DTOs.Prescription;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.SQLserver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.SQLserver.Interfaces
{
    public interface IPrescriptionService
    {
        Task<ApiResponse<Prescription>> CreatePrescriptionAsync(CreatePrescriptionDto dto);
        Task<ApiResponse<Prescription>> GetPrescriptionByIdAsync(Guid id);
        Task<ApiResponse<List<Prescription>>> GetPrescriptionsByDossierAsync(Guid dossierId);
        Task<ApiResponse<List<Prescription>>> GetPrescriptionsByMedecinAsync(Guid medecinId);
        Task<ApiResponse<Prescription>> UpdatePrescriptionAsync(UpdatePrescriptionDto dto);
        Task<ApiResponse<bool>> DeletePrescriptionAsync(Guid id);
    }
}
