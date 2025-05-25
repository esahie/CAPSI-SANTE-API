using CAPSI.Sante.Application.DTOs.AntecedentMedical;
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
    public class AntecedentMedicalService : IAntecedentMedicalService
    {
        private readonly IAntecedentMedicalRepository _antecedentRepository;
        private readonly IDossierMedicalRepository _dossierRepository;
        private readonly ILogger<AntecedentMedicalService> _logger;

        public AntecedentMedicalService(
            IAntecedentMedicalRepository antecedentRepository,
            IDossierMedicalRepository dossierRepository,
            ILogger<AntecedentMedicalService> logger)
        {
            _antecedentRepository = antecedentRepository;
            _dossierRepository = dossierRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<AntecedentMedical>> CreateAntecedentAsync(CreateAntecedentMedicalDto dto)
        {
            try
            {
                // Vérifier si le dossier existe
                var dossierExists = await _dossierRepository.ExistsAsync(dto.DossierId);
                if (!dossierExists)
                {
                    return new ApiResponse<AntecedentMedical>
                    {
                        Success = false,
                        Message = $"Dossier médical avec ID {dto.DossierId} non trouvé"
                    };
                }

                // Créer l'antécédent
                var antecedent = new AntecedentMedical
                {
                    DossierId = dto.DossierId,
                    Type = dto.Type,
                    Description = dto.Description,
                    DateDiagnostic = dto.DateDiagnostic ?? DateTime.Now.Date
                };

                var createdAntecedent = await _antecedentRepository.AddAsync(antecedent);

                // Mettre à jour la date de dernière mise à jour du dossier
                var dossier = await _dossierRepository.GetByIdAsync(dto.DossierId);
                dossier.DerniereMiseAJour = DateTime.UtcNow;
                await _dossierRepository.UpdateAsync(dossier);

                return new ApiResponse<AntecedentMedical>
                {
                    Success = true,
                    Message = "Antécédent médical ajouté avec succès",
                    Data = createdAntecedent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'antécédent médical pour le dossier {DossierId}", dto.DossierId);
                return new ApiResponse<AntecedentMedical>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la création de l'antécédent médical"
                };
            }
        }

        public async Task<ApiResponse<AntecedentMedical>> GetAntecedentByIdAsync(Guid id)
        {
            try
            {
                var antecedent = await _antecedentRepository.GetByIdAsync(id);
                if (antecedent == null)
                {
                    return new ApiResponse<AntecedentMedical>
                    {
                        Success = false,
                        Message = $"Antécédent médical avec ID {id} non trouvé"
                    };
                }

                return new ApiResponse<AntecedentMedical>
                {
                    Success = true,
                    Data = antecedent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'antécédent médical {AntecedentId}", id);
                return new ApiResponse<AntecedentMedical>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération de l'antécédent médical"
                };
            }
        }

        public async Task<ApiResponse<List<AntecedentMedical>>> GetAntecedentsByDossierAsync(Guid dossierId, string type = null)
        {
            try
            {
                // Vérifier si le dossier existe
                var dossierExists = await _dossierRepository.ExistsAsync(dossierId);
                if (!dossierExists)
                {
                    return new ApiResponse<List<AntecedentMedical>>
                    {
                        Success = false,
                        Message = $"Dossier médical avec ID {dossierId} non trouvé"
                    };
                }

                // Récupérer les antécédents
                IEnumerable<AntecedentMedical> antecedents;
                if (string.IsNullOrEmpty(type))
                {
                    antecedents = await _antecedentRepository.GetByDossierIdAsync(dossierId);
                }
                else
                {
                    antecedents = await _antecedentRepository.GetByTypeAsync(dossierId, type);
                }

                return new ApiResponse<List<AntecedentMedical>>
                {
                    Success = true,
                    Data = antecedents.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des antécédents médicaux pour le dossier {DossierId}", dossierId);
                return new ApiResponse<List<AntecedentMedical>>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des antécédents médicaux"
                };
            }
        }

        public async Task<ApiResponse<AntecedentMedical>> UpdateAntecedentAsync(UpdateAntecedentMedicalDto dto)
        {
            try
            {
                // Vérifier si l'antécédent existe
                var existingAntecedent = await _antecedentRepository.GetByIdAsync(dto.Id);
                if (existingAntecedent == null)
                {
                    return new ApiResponse<AntecedentMedical>
                    {
                        Success = false,
                        Message = $"Antécédent médical avec ID {dto.Id} non trouvé"
                    };
                }

                // Mettre à jour l'antécédent
                existingAntecedent.Type = dto.Type;
                existingAntecedent.Description = dto.Description;
                existingAntecedent.DateDiagnostic = dto.DateDiagnostic ?? existingAntecedent.DateDiagnostic;

                var updatedAntecedent = await _antecedentRepository.UpdateAsync(existingAntecedent);

                // Mettre à jour la date de dernière mise à jour du dossier
                var dossier = await _dossierRepository.GetByIdAsync(existingAntecedent.DossierId);
                dossier.DerniereMiseAJour = DateTime.UtcNow;
                await _dossierRepository.UpdateAsync(dossier);

                return new ApiResponse<AntecedentMedical>
                {
                    Success = true,
                    Message = "Antécédent médical mis à jour avec succès",
                    Data = updatedAntecedent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'antécédent médical {AntecedentId}", dto.Id);
                return new ApiResponse<AntecedentMedical>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la mise à jour de l'antécédent médical"
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteAntecedentAsync(Guid id)
        {
            try
            {
                // Vérifier si l'antécédent existe
                var antecedent = await _antecedentRepository.GetByIdAsync(id);
                if (antecedent == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Antécédent médical avec ID {id} non trouvé",
                        Data = false
                    };
                }

                // Mettre à jour la date de dernière mise à jour du dossier avant de supprimer l'antécédent
                var dossier = await _dossierRepository.GetByIdAsync(antecedent.DossierId);
                dossier.DerniereMiseAJour = DateTime.UtcNow;
                await _dossierRepository.UpdateAsync(dossier);

                // Supprimer l'antécédent
                await _antecedentRepository.DeleteAsync(id);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Antécédent médical supprimé avec succès",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'antécédent médical {AntecedentId}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la suppression de l'antécédent médical",
                    Data = false
                };
            }
        }
    }
}
