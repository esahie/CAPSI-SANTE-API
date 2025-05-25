using CAPSI.Sante.Application.DTOs.ServiceMedical;
using CAPSI.Sante.Application.Data;
using CAPSI.Sante.Common;
using Microsoft.Extensions.Logging;
using CAPSI.Sante.Domain.Models.SQLserver;
using CAPSI.Sante.Application.Services.SQLserver.Interfaces;
using CAPSI.Sante.Application.Services.Firebase.Interfaces;
using CAPSI.Sante.Application.Repositories.SQLserver.Interfaces;

namespace CAPSI.Sante.Application.Services.SQLserver
{
    public class ServiceMedicalService : IServiceMedicalService
    {
        private readonly IServiceMedicalRepository _serviceRepository;
        private readonly IFirestoreRendezVousService _firestoreRendezVousService; // Utilisez l'interface
        private readonly ILogger<ServiceMedicalService> _logger;

        public ServiceMedicalService(
            IServiceMedicalRepository serviceRepository,
            IFirestoreRendezVousService firestoreRendezVousService, // Injectez l'interface
            ILogger<ServiceMedicalService> logger)
        {
            _serviceRepository = serviceRepository;
            _firestoreRendezVousService = firestoreRendezVousService;
            _logger = logger;
        }


        public async Task<ApiResponse<ServiceMedical>> CreateServiceAsync(CreateServiceMedicalDto dto)
        {
            try
            {
                // Vérifier si le code existe déjà
                var existingService = await _serviceRepository.GetByCodeAsync(dto.Code);
                if (existingService != null)
                {
                    return new ApiResponse<ServiceMedical>
                    {
                        Success = false,
                        Message = "Un service avec ce code existe déjà"
                    };
                }

                var service = new ServiceMedical
                {
                    Code = dto.Code,
                    Nom = dto.Nom,
                    Description = dto.Description,
                    DureeParDefaut = dto.DureeParDefaut,
                    Tarif = dto.Tarif,
                    RequiertAssurance = dto.RequiertAssurance,
                    EstActif = true, // Par défaut, un nouveau service est actif
                    CreatedAt = DateTime.UtcNow
                };

                service = await _serviceRepository.AddAsync(service);

                return new ApiResponse<ServiceMedical>
                {
                    Success = true,
                    Data = service,
                    Message = "Service médical créé avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du service médical");
                return new ApiResponse<ServiceMedical>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la création du service"
                };
            }
        }

        public async Task<ApiResponse<ServiceMedical>> GetServiceByIdAsync(Guid id)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAsync(id);
                if (service == null)
                {
                    return new ApiResponse<ServiceMedical>
                    {
                        Success = false,
                        Message = "Service non trouvé"
                    };
                }

                return new ApiResponse<ServiceMedical>
                {
                    Success = true,
                    Data = service
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du service {ServiceId}", id);
                return new ApiResponse<ServiceMedical>
                {
                    Success = false,
                    Message = "Une erreur est survenue"
                };
            }
        }

        public async Task<ApiResponse<ServiceMedical>> GetServiceByCodeAsync(string code)
        {
            try
            {
                var service = await _serviceRepository.GetByCodeAsync(code);
                if (service == null)
                {
                    return new ApiResponse<ServiceMedical>
                    {
                        Success = false,
                        Message = $"Service avec le code {code} non trouvé"
                    };
                }

                return new ApiResponse<ServiceMedical>
                {
                    Success = true,
                    Data = service
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du service par code {Code}", code);
                return new ApiResponse<ServiceMedical>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération du service"
                };
            }
        }

        public async Task<ApiResponse<List<ServiceMedical>>> GetAllServicesAsync(bool includeInactive = false)
        {
            try
            {
                var services = await _serviceRepository.GetAllAsync(includeInactive);
                return new ApiResponse<List<ServiceMedical>>
                {
                    Success = true,
                    Data = services.ToList(),
                    Message = $"{services.Count()} services trouvés"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de tous les services");
                return new ApiResponse<List<ServiceMedical>>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des services"
                };
            }
        }

        public async Task<ApiResponse<List<ServiceMedical>>> GetServicesByMedecinAsync(Guid medecinId, bool includeInactive = false)
        {
            try
            {
                var services = await _serviceRepository.GetByMedecinAsync(medecinId, includeInactive);
                return new ApiResponse<List<ServiceMedical>>
                {
                    Success = true,
                    Data = services.ToList(),
                    Message = $"{services.Count()} services trouvés pour ce médecin"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des services du médecin {MedecinId}", medecinId);
                return new ApiResponse<List<ServiceMedical>>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des services"
                };
            }
        }

        public async Task<ApiResponse<ServiceMedical>> UpdateServiceAsync(UpdateServiceMedicalDto dto)
        {
            try
            {
                var existingService = await _serviceRepository.GetByIdAsync(dto.Id);
                if (existingService == null)
                {
                    return new ApiResponse<ServiceMedical>
                    {
                        Success = false,
                        Message = "Service non trouvé"
                    };
                }

                // Vérifier si le nouveau code n'existe pas déjà pour un autre service
                if (dto.Code != existingService.Code)
                {
                    var serviceWithSameCode = await _serviceRepository.GetByCodeAsync(dto.Code);
                    if (serviceWithSameCode != null && serviceWithSameCode.Id != dto.Id)
                    {
                        return new ApiResponse<ServiceMedical>
                        {
                            Success = false,
                            Message = "Un service avec ce code existe déjà"
                        };
                    }
                }

                // Mise à jour des propriétés
                existingService.Code = dto.Code;
                existingService.Nom = dto.Nom;
                existingService.Description = dto.Description;
                existingService.DureeParDefaut = dto.DureeParDefaut;
                existingService.Tarif = dto.Tarif;
                existingService.RequiertAssurance = dto.RequiertAssurance;
                existingService.EstActif = dto.EstActif;
                existingService.UpdatedAt = DateTime.UtcNow;

                await _serviceRepository.UpdateAsync(existingService);

                return new ApiResponse<ServiceMedical>
                {
                    Success = true,
                    Data = existingService,
                    Message = "Service mis à jour avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du service {ServiceId}", dto.Id);
                return new ApiResponse<ServiceMedical>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la mise à jour du service"
                };
            }
        }

        public async Task<ApiResponse<ServiceMedical>> ActivateServiceAsync(Guid id)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAsync(id);
                if (service == null)
                {
                    return new ApiResponse<ServiceMedical>
                    {
                        Success = false,
                        Message = "Service non trouvé"
                    };
                }

                if (service.EstActif)
                {
                    return new ApiResponse<ServiceMedical>
                    {
                        Success = true,
                        Data = service,
                        Message = "Le service est déjà actif"
                    };
                }

                service = await _serviceRepository.ActivateAsync(id);

                return new ApiResponse<ServiceMedical>
                {
                    Success = true,
                    Data = service,
                    Message = "Service activé avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'activation du service {ServiceId}", id);
                return new ApiResponse<ServiceMedical>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de l'activation du service"
                };
            }
        }

        public async Task<ApiResponse<ServiceMedical>> DeactivateServiceAsync(Guid id)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAsync(id);
                if (service == null)
                {
                    return new ApiResponse<ServiceMedical>
                    {
                        Success = false,
                        Message = "Service non trouvé"
                    };
                }

                if (!service.EstActif)
                {
                    return new ApiResponse<ServiceMedical>
                    {
                        Success = true,
                        Data = service,
                        Message = "Le service est déjà inactif"
                    };
                }

                service = await _serviceRepository.DeactivateAsync(id);

                return new ApiResponse<ServiceMedical>
                {
                    Success = true,
                    Data = service,
                    Message = "Service désactivé avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la désactivation du service {ServiceId}", id);
                return new ApiResponse<ServiceMedical>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la désactivation du service"
                };
            }
        }

        private async Task<bool> IsServiceUsedInFirebaseAsync(Guid id)
        {
            try
            {
                // Dans votre modèle Firebase actuel, les rendez-vous ont un champ "typeConsultation"
                // qui pourrait correspondre au ServiceMedical.
                // Si les services sont identifiés par leur Code ou Nom dans Firebase,
                // il faut d'abord récupérer ces infos
                var service = await _serviceRepository.GetByIdAsync(id);
                if (service == null)
                    return false;

                // Récupérer tous les rendez-vous et vérifier si le typeConsultation correspond
                // au code du service (cette logique dépend de votre implémentation exacte)
                var rdvs = await _firestoreRendezVousService.GetAllRendezVousAsync();

                // Si votre implémentation n'a pas cette méthode, vous pouvez la créer ou modifier cet exemple
                return rdvs.Any(rdv => rdv.TypeConsultation == service.Code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification d'utilisation du service dans Firebase {ServiceId}", id);
                // En cas d'erreur, par prudence, supposons que le service est utilisé
                return true;
            }
        }

        public async Task<ApiResponse<bool>> DeleteServiceAsync(Guid id)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAsync(id);
                if (service == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Service non trouvé"
                    };
                }

                // Vérifier si le service est utilisé dans Firebase
                var isUsedInFirebase = await IsServiceUsedInFirebaseAsync(id);

                if (isUsedInFirebase)
                {
                    // Si le service est utilisé, désactiver au lieu de supprimer
                    if (service.EstActif)
                    {
                        await _serviceRepository.DeactivateAsync(id);

                        return new ApiResponse<bool>
                        {
                            Success = true,
                            Data = true,
                            Message = "Le service a été désactivé plutôt que supprimé car il est utilisé dans certains rendez-vous. Vous pouvez le réactiver ultérieurement si nécessaire."
                        };
                    }
                    else
                    {
                        return new ApiResponse<bool>
                        {
                            Success = false,
                            Data = false,
                            Message = "Le service est déjà désactivé et ne peut pas être supprimé car il est utilisé dans des rendez-vous."
                        };
                    }
                }

                // Si le service n'est pas utilisé, on peut le supprimer
                await _serviceRepository.DeleteAsync(id);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Service supprimé avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du service {ServiceId}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la suppression du service"
                };
            }
        }

        // Méthode pour la suppression définitive (à utiliser avec précaution)
        public async Task<ApiResponse<bool>> DeleteServicePermanentlyAsync(Guid id)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAsync(id);
                if (service == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Service non trouvé"
                    };
                }

                // Vérifier si le service est utilisé dans Firebase
                var isUsedInFirebase = await IsServiceUsedInFirebaseAsync(id);

                if (isUsedInFirebase)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Le service ne peut pas être supprimé définitivement car il est utilisé dans des rendez-vous. Veuillez le désactiver plutôt que de le supprimer."
                    };
                }

                // Si le service n'est pas utilisé, on peut le supprimer
                await _serviceRepository.DeleteAsync(id);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Service supprimé définitivement avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression définitive du service {ServiceId}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la suppression définitive du service"
                };
            }
        }

        //public async Task<ApiResponse<bool>> DeleteServiceAsync(Guid id)
        //{
        //    try
        //    {
        //        var service = await _serviceRepository.GetByIdAsync(id);
        //        if (service == null)
        //        {
        //            return new ApiResponse<bool>
        //            {
        //                Success = false,
        //                Message = "Service non trouvé"
        //            };
        //        }

        //        // Vérifier si le service est utilisé dans des rendez-vous
        //        var isUsedInAppointments = await _serviceRepository.IsUsedInAppointmentsAsync(id);
        //        if (isUsedInAppointments)
        //        {
        //            return new ApiResponse<bool>
        //            {
        //                Success = false,
        //                Message = "Ce service ne peut pas être supprimé car il est utilisé dans des rendez-vous. Vous pouvez le désactiver à la place."
        //            };
        //        }

        //        await _serviceRepository.DeleteAsync(id);

        //        return new ApiResponse<bool>
        //        {
        //            Success = true,
        //            Data = true,
        //            Message = "Service supprimé avec succès"
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erreur lors de la suppression du service {ServiceId}", id);
        //        return new ApiResponse<bool>
        //        {
        //            Success = false,
        //            Message = "Une erreur est survenue lors de la suppression du service"
        //        };
        //    }
        //}
    }
}