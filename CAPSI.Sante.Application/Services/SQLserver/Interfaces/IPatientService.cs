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
        
        // Nouvelles méthodes pour la gestion des photos
        Task<ApiResponse<bool>> UpdatePatientPhotoAsync(UpdatePatientPhotoDto dto);
        Task<ApiResponse<bool>> DeletePatientPhotoAsync(Guid patientId);
        Task<ApiResponse<string>> GetPatientPhotoAsync(Guid patientId);

        // Nouvelles méthodes pour réactivation
        Task<ApiResponse<string>> RequestReactivationAsync(string email, string motifDemande = null);
        Task<ApiResponse<Patient>> ConfirmReactivationAsync(string token); 

        // MÉTHODES DE RECHERCHE PATIENTS INACTIFS - AJOUTEZ CES LIGNES
        Task<ApiResponse<Patient>> FindInactivePatientByEmailAsync(string email);
        Task<ApiResponse<Patient>> FindInactivePatientByTelephoneAsync(string telephone);
        Task<ApiResponse<Patient>> FindInactivePatientByNumeroAssuranceAsync(string numeroAssurance);
        Task<ApiResponse<Patient>> FindInactivePatientByUserIdAsync(Guid userId);
        Task<ApiResponse<Patient>> FindInactivePatientByNomCompletAsync(string nom, string prenom);

        Task<ApiResponse<Patient>> CreatePatientWithPhotoAsync(CreatePatientWithPhotoDto dto);
    }
}
