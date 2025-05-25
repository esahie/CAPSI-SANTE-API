using CAPSI.Sante.Application.DTOs.Medecin;
using CAPSI.Sante.Common;
using CAPSI.Sante.Core.DTOs;
using CAPSI.Sante.Domain.Models.Firestore;
using CAPSI.Sante.Domain.Models.PostgreSQL;
using CAPSI.Sante.Domain.Models.SQLserver;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.SQLserver.Interfaces
{
    public interface IMedecinService
    {
        // Méthodes existantes
        Task<ApiResponse<Medecin>> CreateMedecinAsync(CreateMedecinDto dto);
        Task<ApiResponse<Medecin>> GetMedecinByIdAsync(Guid id);
        Task<ApiResponse<List<Medecin>>> GetMedecinsAsync(string specialite = null, Guid? serviceId = null, int page = 1, int pageSize = 10);
        Task<ApiResponse<List<Medecin>>> GetBySpecialiteAsync(string specialite);
        Task<ApiResponse<Medecin>> UpdateMedecinAsync(UpdateMedecinDto dto);
        Task<ApiResponse<bool>> DeleteMedecinAsync(Guid id);
        Task<ApiResponse<MedecinAnalytics>> GetAnalyticsAsync(Guid medecinId, DateTime dateDebut, DateTime dateFin);
        Task<ApiResponse<bool>> SetDisponibilitesAsync(Guid medecinId, List<DisponibiliteDto> disponibilites);
        Task<ApiResponse<List<Medecin>>> GetByServiceAsync(Guid serviceId);
        Task<ApiResponse<List<CreneauDisponible>>> GetDisponibilitesAsync(Guid medecinId, DateTime dateDebut, DateTime dateFin);

        // Nouvelles méthodes pour la gestion des photos
        Task<ApiResponse<string>> UploadPhotoAsync(Guid medecinId, IFormFile photo);
        Task<ApiResponse<bool>> DeletePhotoAsync(Guid medecinId);
        Task<ApiResponse<Domain.Models.SQLserver.FileInfo>> GetPhotoInfoAsync(Guid medecinId);
    }
}
