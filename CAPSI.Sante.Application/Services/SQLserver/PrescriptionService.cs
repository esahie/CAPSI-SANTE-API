using CAPSI.Sante.Application.DTOs.Prescription;
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
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IDossierMedicalRepository _dossierRepository;
        private readonly IMedecinRepository _medecinRepository;
        private readonly ILogger<PrescriptionService> _logger;

        public PrescriptionService(
            IPrescriptionRepository prescriptionRepository,
            IDossierMedicalRepository dossierRepository,
            IMedecinRepository medecinRepository,
            ILogger<PrescriptionService> logger)
        {
            _prescriptionRepository = prescriptionRepository;
            _dossierRepository = dossierRepository;
            _medecinRepository = medecinRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<Prescription>> CreatePrescriptionAsync(CreatePrescriptionDto dto)
        {
            try
            {
                // Vérifier si le dossier existe
                var dossierExists = await _dossierRepository.ExistsAsync(dto.DossierId);
                if (!dossierExists)
                {
                    return new ApiResponse<Prescription>
                    {
                        Success = false,
                        Message = $"Dossier médical avec ID {dto.DossierId} non trouvé"
                    };
                }

                // Vérifier si le médecin existe
                var medecinExists = await _medecinRepository.ExistsAsync(dto.MedecinId);
                if (!medecinExists)
                {
                    return new ApiResponse<Prescription>
                    {
                        Success = false,
                        Message = $"Médecin avec ID {dto.MedecinId} non trouvé"
                    };
                }

                // Créer la prescription
                var prescription = new Prescription
                {
                    DossierId = dto.DossierId,
                    MedecinId = dto.MedecinId,
                    DatePrescription = DateTime.UtcNow,
                    DateFin = dto.DateFin,
                    Instructions = dto.Instructions,
                    Medicaments = new List<MedicamentPrescrit>()
                };

                // Ajouter les médicaments
                if (dto.Medicaments != null && dto.Medicaments.Any())
                {
                    foreach (var medicamentDto in dto.Medicaments)
                    {
                        prescription.Medicaments.Add(new MedicamentPrescrit
                        {
                            NomMedicament = medicamentDto.NomMedicament,
                            Posologie = medicamentDto.Posologie,
                            Duree = medicamentDto.Duree,
                            Instructions = medicamentDto.Instructions
                        });
                    }
                }

                var createdPrescription = await _prescriptionRepository.AddAsync(prescription);

                // Mettre à jour la date de dernière mise à jour du dossier
                var dossier = await _dossierRepository.GetByIdAsync(dto.DossierId);
                dossier.DerniereMiseAJour = DateTime.UtcNow;
                await _dossierRepository.UpdateAsync(dossier);

                return new ApiResponse<Prescription>
                {
                    Success = true,
                    Message = "Prescription créée avec succès",
                    Data = createdPrescription
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la prescription pour le dossier {DossierId}", dto.DossierId);
                return new ApiResponse<Prescription>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la création de la prescription"
                };
            }
        }

        public async Task<ApiResponse<Prescription>> GetPrescriptionByIdAsync(Guid id)
        {
            try
            {
                var prescription = await _prescriptionRepository.GetByIdAsync(id);
                if (prescription == null)
                {
                    return new ApiResponse<Prescription>
                    {
                        Success = false,
                        Message = $"Prescription avec ID {id} non trouvée"
                    };
                }

                return new ApiResponse<Prescription>
                {
                    Success = true,
                    Data = prescription
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la prescription {PrescriptionId}", id);
                return new ApiResponse<Prescription>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération de la prescription"
                };
            }
        }

        public async Task<ApiResponse<List<Prescription>>> GetPrescriptionsByDossierAsync(Guid dossierId)
        {
            try
            {
                // Vérifier si le dossier existe
                var dossierExists = await _dossierRepository.ExistsAsync(dossierId);
                if (!dossierExists)
                {
                    return new ApiResponse<List<Prescription>>
                    {
                        Success = false,
                        Message = $"Dossier médical avec ID {dossierId} non trouvé"
                    };
                }

                // Récupérer les prescriptions
                var prescriptions = await _prescriptionRepository.GetByDossierIdAsync(dossierId);

                return new ApiResponse<List<Prescription>>
                {
                    Success = true,
                    Data = prescriptions.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des prescriptions pour le dossier {DossierId}", dossierId);
                return new ApiResponse<List<Prescription>>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des prescriptions"
                };
            }
        }

        public async Task<ApiResponse<List<Prescription>>> GetPrescriptionsByMedecinAsync(Guid medecinId)
        {
            try
            {
                // Vérifier si le médecin existe
                var medecinExists = await _medecinRepository.ExistsAsync(medecinId);
                if (!medecinExists)
                {
                    return new ApiResponse<List<Prescription>>
                    {
                        Success = false,
                        Message = $"Médecin avec ID {medecinId} non trouvé"
                    };
                }

                // Récupérer les prescriptions
                var prescriptions = await _prescriptionRepository.GetByMedecinIdAsync(medecinId);

                return new ApiResponse<List<Prescription>>
                {
                    Success = true,
                    Data = prescriptions.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des prescriptions pour le médecin {MedecinId}", medecinId);
                return new ApiResponse<List<Prescription>>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des prescriptions"
                };
            }
        }

        public async Task<ApiResponse<Prescription>> UpdatePrescriptionAsync(UpdatePrescriptionDto dto)
        {
            try
            {
                // Vérifier si la prescription existe
                var existingPrescription = await _prescriptionRepository.GetByIdAsync(dto.Id);
                if (existingPrescription == null)
                {
                    return new ApiResponse<Prescription>
                    {
                        Success = false,
                        Message = $"Prescription avec ID {dto.Id} non trouvée"
                    };
                }

                // Mettre à jour la prescription
                existingPrescription.DateFin = dto.DateFin;
                existingPrescription.Instructions = dto.Instructions;
                existingPrescription.Medicaments = new List<MedicamentPrescrit>();

                // Ajouter les médicaments
                if (dto.Medicaments != null && dto.Medicaments.Any())
                {
                    foreach (var medicamentDto in dto.Medicaments)
                    {
                        existingPrescription.Medicaments.Add(new MedicamentPrescrit
                        {
                            Id = medicamentDto.Id.HasValue ? medicamentDto.Id.Value : Guid.Empty,
                            PrescriptionId = existingPrescription.Id,
                            NomMedicament = medicamentDto.NomMedicament,
                            Posologie = medicamentDto.Posologie,
                            Duree = medicamentDto.Duree,
                            Instructions = medicamentDto.Instructions
                        });
                    }
                }

                var updatedPrescription = await _prescriptionRepository.UpdateAsync(existingPrescription);

                // Mettre à jour la date de dernière mise à jour du dossier
                var dossier = await _dossierRepository.GetByIdAsync(existingPrescription.DossierId);
                dossier.DerniereMiseAJour = DateTime.UtcNow;
                await _dossierRepository.UpdateAsync(dossier);

                return new ApiResponse<Prescription>
                {
                    Success = true,
                    Message = "Prescription mise à jour avec succès",
                    Data = updatedPrescription
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la prescription {PrescriptionId}", dto.Id);
                return new ApiResponse<Prescription>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la mise à jour de la prescription"
                };
            }
        }

        public async Task<ApiResponse<bool>> DeletePrescriptionAsync(Guid id)
        {
            try
            {
                // Vérifier si la prescription existe
                var prescription = await _prescriptionRepository.GetByIdAsync(id);
                if (prescription == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Prescription avec ID {id} non trouvée",
                        Data = false
                    };
                }

                // Mettre à jour la date de dernière mise à jour du dossier avant de supprimer la prescription
                var dossier = await _dossierRepository.GetByIdAsync(prescription.DossierId);
                dossier.DerniereMiseAJour = DateTime.UtcNow;
                await _dossierRepository.UpdateAsync(dossier);

                // Supprimer la prescription
                await _prescriptionRepository.DeleteAsync(id);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Prescription supprimée avec succès",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la prescription {PrescriptionId}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la suppression de la prescription",
                    Data = false
                };
            }
        }
    }
}
