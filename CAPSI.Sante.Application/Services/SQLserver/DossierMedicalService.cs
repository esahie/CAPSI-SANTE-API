using CAPSI.Sante.Application.DTOs.DossierMedical;
using CAPSI.Sante.Application.Repositories.SQLserver.Interfaces;
using CAPSI.Sante.Application.Services.SQLserver.Interfaces;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.SQLserver;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.SQLserver
{
    public class DossierMedicalService : IDossierMedicalService
    {
        private readonly IDossierMedicalRepository _dossierRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ILogger<DossierMedicalService> _logger;

        public DossierMedicalService(
            IDossierMedicalRepository dossierRepository,
            IPatientRepository patientRepository,
            ILogger<DossierMedicalService> logger)
        {
            _dossierRepository = dossierRepository;
            _patientRepository = patientRepository;
            _logger = logger;
        }

        //public async Task<ApiResponse<DossierMedical>> CreateDossierAsync(CreateDossierMedicalDto dto)
        //{
        //    try
        //    {
        //        // Vérifier si le patient existe
        //        var patientExists = await _patientRepository.ExistsAsync(dto.PatientId);
        //        if (!patientExists)
        //        {
        //            return new ApiResponse<DossierMedical>
        //            {
        //                Success = false,
        //                Message = $"Patient avec ID {dto.PatientId} non trouvé"
        //            };
        //        }

        //        // Vérifier si un dossier existe déjà pour ce patient
        //        var existingDossier = await _dossierRepository.GetByPatientIdAsync(dto.PatientId);
        //        if (existingDossier != null)
        //        {
        //            return new ApiResponse<DossierMedical>
        //            {
        //                Success = false,
        //                Message = $"Un dossier médical existe déjà pour le patient avec ID {dto.PatientId}"
        //            };
        //        }

        //        // Créer le dossier
        //        var dossier = new DossierMedical
        //        {
        //            PatientId = dto.PatientId,
        //            DateCreation = DateTime.UtcNow,
        //            DerniereMiseAJour = DateTime.UtcNow
        //        };

        //        var createdDossier = await _dossierRepository.AddAsync(dossier);

        //        return new ApiResponse<DossierMedical>
        //        {
        //            Success = true,
        //            Message = "Dossier médical créé avec succès",
        //            Data = createdDossier
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erreur lors de la création du dossier médical pour le patient {PatientId}", dto.PatientId);
        //        return new ApiResponse<DossierMedical>
        //        {
        //            Success = false,
        //            Message = "Une erreur est survenue lors de la création du dossier médical"
        //        };
        //    }
        //}

        public async Task<ApiResponse<DossierMedical>> GetDossierByIdAsync(Guid id)
        {
            try
            {
                var dossier = await _dossierRepository.GetByIdAsync(id);
                if (dossier == null)
                {
                    return new ApiResponse<DossierMedical>
                    {
                        Success = false,
                        Message = $"Dossier médical avec ID {id} non trouvé"
                    };
                }

                return new ApiResponse<DossierMedical>
                {
                    Success = true,
                    Data = dossier
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du dossier médical {DossierId}", id);
                return new ApiResponse<DossierMedical>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération du dossier médical"
                };
            }
        }

        public async Task<ApiResponse<DossierMedical>> GetDossierByPatientIdAsync(Guid patientId)
        {
            try
            {
                var dossier = await _dossierRepository.GetByPatientIdAsync(patientId);
                if (dossier == null)
                {
                    return new ApiResponse<DossierMedical>
                    {
                        Success = false,
                        Message = $"Aucun dossier médical trouvé pour le patient avec ID {patientId}"
                    };
                }

                return new ApiResponse<DossierMedical>
                {
                    Success = true,
                    Data = dossier
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du dossier médical pour le patient {PatientId}", patientId);
                return new ApiResponse<DossierMedical>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération du dossier médical"
                };
            }
        }

        public async Task<ApiResponse<List<DossierMedical>>> GetAllDossiersAsync()
        {
            try
            {
                var dossiers = await _dossierRepository.GetAllAsync();
                return new ApiResponse<List<DossierMedical>>
                {
                    Success = true,
                    Data = dossiers.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de tous les dossiers médicaux");
                return new ApiResponse<List<DossierMedical>>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des dossiers médicaux"
                };
            }
        }

        public async Task<ApiResponse<DossierMedical>> UpdateDossierAsync(UpdateDossierMedicalDto dto)
        {
            try
            {
                // Vérifier si le dossier existe
                var existingDossier = await _dossierRepository.GetByIdAsync(dto.Id);
                if (existingDossier == null)
                {
                    return new ApiResponse<DossierMedical>
                    {
                        Success = false,
                        Message = $"Dossier médical avec ID {dto.Id} non trouvé"
                    };
                }

                // Vérifier si le patient existe
                var patientExists = await _patientRepository.ExistsAsync(dto.PatientId);
                if (!patientExists)
                {
                    return new ApiResponse<DossierMedical>
                    {
                        Success = false,
                        Message = $"Patient avec ID {dto.PatientId} non trouvé"
                    };
                }

                // Mettre à jour le dossier
                existingDossier.PatientId = dto.PatientId;
                existingDossier.DerniereMiseAJour = DateTime.UtcNow;

                var updatedDossier = await _dossierRepository.UpdateAsync(existingDossier);

                return new ApiResponse<DossierMedical>
                {
                    Success = true,
                    Message = "Dossier médical mis à jour avec succès",
                    Data = updatedDossier
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du dossier médical {DossierId}", dto.Id);
                return new ApiResponse<DossierMedical>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la mise à jour du dossier médical"
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteDossierAsync(Guid id)
        {
            try
            {
                // Vérifier si le dossier existe
                var dossierExists = await _dossierRepository.ExistsAsync(id);
                if (!dossierExists)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Dossier médical avec ID {id} non trouvé",
                        Data = false
                    };
                }

                // Supprimer le dossier
                await _dossierRepository.DeleteAsync(id);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Dossier médical supprimé avec succès",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du dossier médical {DossierId}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la suppression du dossier médical",
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<DossierMedical>> CreateDossierAsync(CreateDossierMedicalDto dto)
        {
            try
            {
                // Vérifier si le patient existe
                var patientExists = await _patientRepository.ExistsAsync(dto.PatientId);
                if (!patientExists)
                {
                    return new ApiResponse<DossierMedical>
                    {
                        Success = false,
                        Message = $"Patient avec ID {dto.PatientId} non trouvé"
                    };
                }

                // Vérifier si un dossier existe déjà pour ce patient
                var existingDossier = await _dossierRepository.GetByPatientIdAsync(dto.PatientId);
                if (existingDossier != null)
                {
                    return new ApiResponse<DossierMedical>
                    {
                        Success = false,
                        Message = $"Un dossier médical existe déjà pour le patient avec ID {dto.PatientId}"
                    };
                }

                // Créer le dossier
                var dossier = new DossierMedical
                {
                    PatientId = dto.PatientId,
                    DateCreation = DateTime.UtcNow,
                    DerniereMiseAJour = DateTime.UtcNow
                };

                var createdDossier = await _dossierRepository.AddAsync(dossier);

                return new ApiResponse<DossierMedical>
                {
                    Success = true,
                    Message = "Dossier médical créé avec succès",
                    Data = createdDossier
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du dossier médical pour le patient {PatientId}", dto.PatientId);
                return new ApiResponse<DossierMedical>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la création du dossier médical"
                };
            }
        }
    }
}
