using CAPSI.Sante.Application.DTOs.Patient;
using CAPSI.Sante.Application.Repositories.SQLserver.Interfaces;
using CAPSI.Sante.Application.Services.SQLserver.Interfaces;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models;
using CAPSI.Sante.Domain.Models.SQLserver;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.SQLserver
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ILogger<PatientService> _logger;
        private readonly IEmailService _emailService;

        public PatientService(IPatientRepository patientRepository, 
            ILogger<PatientService> logger, IEmailService emailService)
        {
            _patientRepository = patientRepository;
            _logger = logger;
            _emailService = emailService;
        }


        public async Task<ApiResponse<Patient>> CreatePatientAsync(CreatePatientDto dto)
        {
            try
            {
                // Validation de base
                var validation = ValidatePatient(dto);
                if (!validation.IsValid)
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Errors = validation.Errors
                    };
                }

                var errors = new List<string>();
                var suggestions = new List<string>();

                // 1. Vérifier unicité du numéro d'assurance (patients actifs)
                var isNumeroAssuranceUnique = await _patientRepository.IsNumeroAssuranceUnique(dto.NumeroAssuranceMaladie);
                if (!isNumeroAssuranceUnique)
                {
                    var existingByNumero = await _patientRepository.GetByNumeroAssuranceAsync(dto.NumeroAssuranceMaladie);
                    errors.Add($"Ce numéro d'assurance maladie est déjà utilisé par {existingByNumero.Prenom} {existingByNumero.Nom}");
                }
                else
                {
                    // Vérifier si un patient inactif a ce numéro
                    var inactiveByNumero = await _patientRepository.FindInactivePatientByNumeroAssuranceAsync(dto.NumeroAssuranceMaladie);
                    if (inactiveByNumero != null)
                    {
                        suggestions.Add($"Un patient désactivé avec ce numéro d'assurance existe déjà ({inactiveByNumero.Prenom} {inactiveByNumero.Nom}). Voulez-vous demander la réactivation de ce compte ?");
                    }
                }

                // 2. Vérifier unicité de l'email (si fourni)
                if (!string.IsNullOrEmpty(dto.Email))
                {
                    var isEmailUnique = await _patientRepository.IsEmailUniqueAsync(dto.Email);
                    if (!isEmailUnique)
                    {
                        var existingByEmail = await _patientRepository.GetByEmailAsync(dto.Email);
                        errors.Add($"Cet email est déjà utilisé par un patient actif ({existingByEmail.Prenom} {existingByEmail.Nom}). Utilisez un autre email ou connectez-vous avec le compte existant.");
                    }
                    else
                    {
                        // Vérifier si un patient inactif a cet email
                        var inactiveByEmail = await _patientRepository.FindInactivePatientByEmailAsync(dto.Email);
                        if (inactiveByEmail != null)
                        {
                            suggestions.Add($"Un patient désactivé avec cet email existe déjà ({inactiveByEmail.Prenom} {inactiveByEmail.Nom}). Voulez-vous demander la réactivation de ce compte ? Un email de vérification sera envoyé.");
                        }
                    }
                }

                // 3. Vérifier unicité du téléphone (si fourni)
                if (!string.IsNullOrEmpty(dto.Telephone))
                {
                    var isTelephoneUnique = await _patientRepository.IsTelephoneUniqueAsync(dto.Telephone);
                    if (!isTelephoneUnique)
                    {
                        var existingByPhone = await _patientRepository.GetByTelephoneAsync(dto.Telephone);
                        errors.Add($"Ce numéro de téléphone est déjà utilisé par un patient actif ({existingByPhone.Prenom} {existingByPhone.Nom}). Utilisez un autre numéro ou connectez-vous avec le compte existant.");
                    }
                    else
                    {
                        // Vérifier si un patient inactif a ce téléphone
                        var inactiveByPhone = await _patientRepository.FindInactivePatientByTelephoneAsync(dto.Telephone);
                        if (inactiveByPhone != null)
                        {
                            suggestions.Add($"Un patient désactivé avec ce téléphone existe déjà ({inactiveByPhone.Prenom} {inactiveByPhone.Nom}). Voulez-vous demander la réactivation de ce compte ?");
                        }
                    }
                }

                // 4. Vérifier unicité du UserId (si fourni)
                if (dto.UserId.HasValue)
                {
                    var isUserIdUnique = await _patientRepository.IsUserIdUniqueAsync(dto.UserId);
                    if (!isUserIdUnique)
                    {
                        var existingByUserId = await _patientRepository.GetByUserIdAsync(dto.UserId.Value);
                        errors.Add($"Ce compte utilisateur est déjà lié au patient actif {existingByUserId.Prenom} {existingByUserId.Nom}. Utilisez un autre compte ou connectez-vous avec le compte existant.");
                    }
                    else
                    {
                        // Vérifier si un patient inactif a ce UserId
                        var inactiveByUserId = await _patientRepository.FindInactivePatientByUserIdAsync(dto.UserId.Value);
                        if (inactiveByUserId != null)
                        {
                            suggestions.Add($"Un patient désactivé avec ce compte utilisateur existe déjà ({inactiveByUserId.Prenom} {inactiveByUserId.Nom}). Voulez-vous demander la réactivation de ce compte ?");
                        }
                    }
                }

                // 5. Vérifier nom complet pour patients inactifs
                var inactiveByNomComplet = await _patientRepository.FindInactivePatientByNomCompletAsync(dto.Nom, dto.Prenom);
                if (inactiveByNomComplet != null)
                {
                    suggestions.Add($"Un patient désactivé avec le même nom complet existe déjà ({inactiveByNomComplet.Prenom} {inactiveByNomComplet.Nom}, né le {inactiveByNomComplet.DateNaissance:dd/MM/yyyy}). S'agit-il de la même personne ? Voulez-vous demander la réactivation ?");
                }

                // Si des erreurs d'unicité sont trouvées, les retourner
                if (errors.Any())
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Des doublons ont été détectés avec des patients actifs",
                        Errors = errors.Concat(suggestions).ToList()
                    };
                }

                // Si seulement des suggestions (patients inactifs), proposer réactivation
                if (suggestions.Any())
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Des patients désactivés similaires ont été trouvés",
                        Errors = suggestions.Concat(new[] {
                    "Pour continuer la création, utilisez l'endpoint POST /api/patient/request-reactivation avec l'email du patient désactivé.",
                    "Ou contactez l'administrateur pour réactiver manuellement le compte existant.",
                    "Si vous êtes sûr de vouloir créer un nouveau patient, modifiez les informations en conflit."
                }).ToList()
                    };
                }

                // Si tout est unique, créer le patient
                var patient = new Patient
                {
                    UserId = dto.UserId,
                    NumeroAssuranceMaladie = dto.NumeroAssuranceMaladie,
                    Nom = dto.Nom,
                    Prenom = dto.Prenom,
                    DateNaissance = dto.DateNaissance,
                    Sexe = dto.Sexe,
                    Telephone = dto.Telephone,
                    Email = dto.Email,
                    Adresse = dto.Adresse,
                    CodePostal = dto.CodePostal,
                    Ville = dto.Ville,
                    GroupeSanguin = dto.GroupeSanguin,
                    PhotoUrl = dto.PhotoUrl,
                    CreatedAt = DateTime.UtcNow
                };

                patient = await _patientRepository.AddAsync(patient);

                return new ApiResponse<Patient>
                {
                    Success = true,
                    Data = patient,
                    Message = "Patient créé avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du patient");
                return new ApiResponse<Patient>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la création du patient"
                };
            }
        }

 
        public async Task<ApiResponse<Patient>> GetPatientByIdAsync(Guid id)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(id);
                if (patient == null)
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Patient non trouvé"
                    };
                }

                return new ApiResponse<Patient>
                {
                    Success = true,
                    Data = patient
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du patient {PatientId}", id);
                return new ApiResponse<Patient>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération du patient"
                };
            }
        }

        public async Task<ApiResponse<List<Patient>>> GetPatientsAsync(string searchTerm = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var patients = string.IsNullOrEmpty(searchTerm)
                    ? await _patientRepository.GetAllAsync()
                    : await _patientRepository.SearchAsync(searchTerm);

                var paginatedPatients = patients
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new ApiResponse<List<Patient>>
                {
                    Success = true,
                    Data = paginatedPatients,
                    Message = $"{paginatedPatients.Count} patients trouvés"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des patients");
                return new ApiResponse<List<Patient>>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des patients"
                };
            }
        }

        public async Task<ApiResponse<Patient>> UpdatePatientAsync(UpdatePatientDto dto)
        {
            try
            {
                var existingPatient = await _patientRepository.GetByIdAsync(dto.Id);
                if (existingPatient == null)
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Patient non trouvé"
                    };
                }

                var errors = new List<string>();

                // 1. Vérifier unicité du numéro d'assurance (si changé)
                if (dto.NumeroAssuranceMaladie != existingPatient.NumeroAssuranceMaladie)
                {
                    var isNumeroAssuranceUnique = await _patientRepository.IsNumeroAssuranceUnique(dto.NumeroAssuranceMaladie, dto.Id);
                    if (!isNumeroAssuranceUnique)
                    {
                        var existingByNumero = await _patientRepository.GetByNumeroAssuranceAsync(dto.NumeroAssuranceMaladie);
                        errors.Add($"Ce numéro d'assurance maladie est déjà utilisé par {existingByNumero.Prenom} {existingByNumero.Nom}");
                    }
                }

                // 2. Vérifier unicité de l'email (si changé)
                if (dto.Email != existingPatient.Email && !string.IsNullOrEmpty(dto.Email))
                {
                    var isEmailUnique = await _patientRepository.IsEmailUniqueAsync(dto.Email, dto.Id);
                    if (!isEmailUnique)
                    {
                        var existingByEmail = await _patientRepository.GetByEmailAsync(dto.Email);
                        errors.Add($"Cet email est déjà utilisé par {existingByEmail.Prenom} {existingByEmail.Nom}");
                    }
                }

                // 3. Vérifier unicité du téléphone (si changé)
                if (dto.Telephone != existingPatient.Telephone && !string.IsNullOrEmpty(dto.Telephone))
                {
                    var isTelephoneUnique = await _patientRepository.IsTelephoneUniqueAsync(dto.Telephone, dto.Id);
                    if (!isTelephoneUnique)
                    {
                        var existingByPhone = await _patientRepository.GetByTelephoneAsync(dto.Telephone);
                        errors.Add($"Ce numéro de téléphone est déjà utilisé par {existingByPhone.Prenom} {existingByPhone.Nom}");
                    }
                }

                // 4. Vérifier unicité du UserId (si changé)
                if (dto.UserId != existingPatient.UserId && dto.UserId.HasValue)
                {
                    var isUserIdUnique = await _patientRepository.IsUserIdUniqueAsync(dto.UserId, dto.Id);
                    if (!isUserIdUnique)
                    {
                        var existingByUserId = await _patientRepository.GetByUserIdAsync(dto.UserId.Value);
                        errors.Add($"Ce compte utilisateur est déjà lié au patient {existingByUserId.Prenom} {existingByUserId.Nom}");
                    }
                }

                // Si des erreurs d'unicité sont trouvées, les retourner
                if (errors.Any())
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Des doublons ont été détectés",
                        Errors = errors
                    };
                }

                // Si tout est unique, mettre à jour le patient
                existingPatient.Id = dto.Id;
                existingPatient.NumeroAssuranceMaladie = dto.NumeroAssuranceMaladie;
                existingPatient.Nom = dto.Nom;
                existingPatient.Prenom = dto.Prenom;
                existingPatient.DateNaissance = dto.DateNaissance;
                existingPatient.Sexe = dto.Sexe;
                existingPatient.Telephone = dto.Telephone;
                existingPatient.Email = dto.Email;
                existingPatient.Adresse = dto.Adresse;
                existingPatient.CodePostal = dto.CodePostal;
                existingPatient.Ville = dto.Ville;
                existingPatient.GroupeSanguin = dto.GroupeSanguin;
                existingPatient.PhotoUrl = dto.PhotoUrl;
                existingPatient.UpdatedAt = DateTime.UtcNow;

                await _patientRepository.UpdateAsync(existingPatient);

                return new ApiResponse<Patient>
                {
                    Success = true,
                    Data = existingPatient,
                    Message = "Patient mis à jour avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du patient {PatientId}", dto.Id);
                return new ApiResponse<Patient>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la mise à jour du patient"
                };
            }
        }
        
        public async Task<ApiResponse<bool>> DeletePatientAsync(Guid id)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(id);
                if (patient == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Patient non trouvé"
                    };
                }

                await _patientRepository.DeleteAsync(id);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Patient supprimé avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du patient {PatientId}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la suppression du patient"
                };
            }
        }

        public async Task<ApiResponse<Patient>> GetByNumeroAssuranceAsync(string numeroAssurance)
        {
            try
            {
                var patient = await _patientRepository.GetByNumeroAssuranceAsync(numeroAssurance);
                if (patient == null)
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Patient non trouvé"
                    };
                }

                return new ApiResponse<Patient>
                {
                    Success = true,
                    Data = patient
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche du patient par numéro d'assurance {NumeroAssurance}", numeroAssurance);
                return new ApiResponse<Patient>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la recherche du patient"
                };
            }
        }

        public async Task<ApiResponse<List<Patient>>> SearchPatientsAsync(string searchTerm)
        {
            try
            {
                var patients = await _patientRepository.SearchAsync(searchTerm);
                return new ApiResponse<List<Patient>>
                {
                    Success = true,
                    Data = patients.ToList(),
                    Message = $"{patients.Count()} patients trouvés"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche des patients");
                return new ApiResponse<List<Patient>>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la recherche des patients"
                };
            }
        }

        private ValidationResult ValidatePatient(CreatePatientDto dto)
        {
            var result = new ValidationResult();

            // Validation du nom et prénom
            if (string.IsNullOrEmpty(dto.Nom))
                result.Errors.Add("Le nom est requis");
            if (string.IsNullOrEmpty(dto.Prenom))
                result.Errors.Add("Le prénom est requis");

            // Validation de la date de naissance
            if (dto.DateNaissance == default)
                result.Errors.Add("La date de naissance est requise");
            else if (dto.DateNaissance > DateTime.Today)
                result.Errors.Add("La date de naissance ne peut pas être dans le futur");

            // Validation du sexe
            if (!new[] { "M", "F" }.Contains(dto.Sexe))
                result.Errors.Add("Le sexe doit être 'M' ou 'F'");

            // Validation du groupe sanguin si spécifié
            if (!string.IsNullOrEmpty(dto.GroupeSanguin))
            {
                var groupesSanguinsValides = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
                if (!groupesSanguinsValides.Contains(dto.GroupeSanguin))
                    result.Errors.Add("Groupe sanguin invalide");
            }

            // Validation de l'email si spécifié
            if (!string.IsNullOrEmpty(dto.Email) && !Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                result.Errors.Add("Format d'email invalide");

            // Validation du téléphone si spécifié
            if (!string.IsNullOrEmpty(dto.Telephone) && !Regex.IsMatch(dto.Telephone, @"^\+?[\d\s-]{10,}$"))
                result.Errors.Add("Format de téléphone invalide");

            return result;
        }

        public async Task<ApiResponse<bool>> DeactivatePatientAsync(Guid id)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(id);
                if (patient == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Patient non trouvé"
                    };
                }

                if (!patient.EstActif)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Le patient est déjà désactivé"
                    };
                }

                await _patientRepository.DeactivateAsync(id);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Patient désactivé avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la désactivation du patient {PatientId}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la désactivation du patient"
                };
            }
        }

        public async Task<ApiResponse<bool>> ReactivatePatientAsync(Guid id)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(id);
                if (patient == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Patient non trouvé"
                    };
                }

                if (patient.EstActif)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Le patient est déjà actif"
                    };
                }

                await _patientRepository.ReactivateAsync(id);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Patient réactivé avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la réactivation du patient {PatientId}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la réactivation du patient"
                };
            }
        }

        public async Task<ApiResponse<List<Patient>>> GetPatientsAsync(
            string searchTerm = null, int page = 1, int pageSize = 10, bool includeInactive = false)
        {
            try
            {
                var patients = string.IsNullOrEmpty(searchTerm)
                    ? await _patientRepository.GetAllAsync(includeInactive)
                    : await _patientRepository.SearchAsync(searchTerm, includeInactive);

                var paginatedPatients = patients
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new ApiResponse<List<Patient>>
                {
                    Success = true,
                    Data = paginatedPatients,
                    Message = $"{paginatedPatients.Count} patients trouvés"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des patients");
                return new ApiResponse<List<Patient>>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des patients"
                };
            }
        }

        public async Task<ApiResponse<bool>> UpdatePatientPhotoAsync(UpdatePatientPhotoDto dto)
        {
            try
            {
                // Vérifier si le patient existe
                var patient = await _patientRepository.GetByIdAsync(dto.Id);
                if (patient == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Patient non trouvé"
                    };
                }

                // Validation de l'URL de la photo
                if (!IsValidPhotoUrl(dto.PhotoUrl))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "URL de photo invalide"
                    };
                }

                await _patientRepository.UpdatePhotoAsync(dto.Id, dto.PhotoUrl);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Photo du patient mise à jour avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la photo du patient {PatientId}", dto.Id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la mise à jour de la photo"
                };
            }
        }

        // Méthode pour supprimer la photo d'un patient
        public async Task<ApiResponse<bool>> DeletePatientPhotoAsync(Guid patientId)
        {
            try
            {
                var patient = await _patientRepository.GetByIdAsync(patientId);
                if (patient == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Patient non trouvé"
                    };
                }

                if (string.IsNullOrEmpty(patient.PhotoUrl))
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Le patient n'a pas de photo à supprimer"
                    };
                }

                await _patientRepository.DeletePhotoAsync(patientId);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Photo du patient supprimée avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la photo du patient {PatientId}", patientId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la suppression de la photo"
                };
            }
        }

        // Méthode pour récupérer la photo d'un patient
        public async Task<ApiResponse<string>> GetPatientPhotoAsync(Guid patientId)
        {
            try
            {
                var photoUrl = await _patientRepository.GetPhotoUrlAsync(patientId);

                if (string.IsNullOrEmpty(photoUrl))
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Aucune photo trouvée pour ce patient"
                    };
                }

                return new ApiResponse<string>
                {
                    Success = true,
                    Data = photoUrl,
                    Message = "Photo récupérée avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la photo du patient {PatientId}", patientId);
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération de la photo"
                };
            }
        }

        // Méthode de validation de l'URL de la photo
        private bool IsValidPhotoUrl(string photoUrl)
        {
            if (string.IsNullOrEmpty(photoUrl))
                return false;

            // Vérifier si c'est une URL valide
            if (!Uri.TryCreate(photoUrl, UriKind.Absolute, out Uri uriResult))
                return false;

            // Vérifier si c'est HTTP/HTTPS ou un chemin local
            return uriResult.Scheme == Uri.UriSchemeHttp ||
                   uriResult.Scheme == Uri.UriSchemeHttps ||
                   photoUrl.StartsWith("/uploads/");
        }

        // Demander réactivation d'un patient
        //public async Task<ApiResponse<string>> RequestReactivationAsync(string email, string motifDemande = null)
        //{
        //    try
        //    {
        //        // Trouver patient inactif par email
        //        var inactivePatient = await _patientRepository.FindInactivePatientByEmailAsync(email);
        //        if (inactivePatient == null)
        //        {
        //            return new ApiResponse<string>
        //            {
        //                Success = false,
        //                Message = "Aucun patient désactivé trouvé avec cet email"
        //            };
        //        }

        //        // Créer demande de réactivation
        //        var demandeId = await _patientRepository.CreateDemandeReactivationAsync(
        //            inactivePatient.Id,
        //            email,
        //            motifDemande
        //        );

        //        // TODO: Envoyer email de vérification avec lien de confirmation
        //        // Le lien devrait pointer vers : GET /api/patient/confirm-reactivation/{token}

        //        return new ApiResponse<string>
        //        {
        //            Success = true,
        //            Data = demandeId.ToString(),
        //            Message = $"Demande de réactivation créée pour {inactivePatient.Prenom} {inactivePatient.Nom}. Un email de vérification a été envoyé à {email}."
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erreur lors de la demande de réactivation pour {Email}", email);
        //        return new ApiResponse<string>
        //        {
        //            Success = false,
        //            Message = "Une erreur est survenue lors de la demande de réactivation"
        //        };
        //    }
        //}

        // Confirmer réactivation par token

        public async Task<ApiResponse<string>> RequestReactivationAsync(string email, string motifDemande = null)
        {
            try
            {
                // Trouver patient inactif par email
                var inactivePatient = await _patientRepository.FindInactivePatientByEmailAsync(email);
                if (inactivePatient == null)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Aucun patient désactivé trouvé avec cet email"
                    };
                }

                // Créer demande de réactivation
                var demandeId = await _patientRepository.CreateDemandeReactivationAsync(
                    inactivePatient.Id,
                    email,
                    motifDemande
                );

                // ENVOYER L'EMAIL RÉEL - AJOUTEZ CES LIGNES
                var emailSent = await _emailService.SendReactivationEmailAsync(
                    email,
                    $"{inactivePatient.Prenom} {inactivePatient.Nom}",
                    demandeId.ToString("N") // Token de réactivation
                );

                if (!emailSent)
                {
                    _logger.LogWarning("Échec de l'envoi de l'email de réactivation à {Email}", email);
                    // Ne pas échouer la demande si l'email échoue
                }

                // ENVOYER NOTIFICATION ADMIN - AJOUTEZ CES LIGNES
                await _emailService.SendAdminNotificationAsync(
                    $"{inactivePatient.Prenom} {inactivePatient.Nom}",
                    motifDemande
                );

                return new ApiResponse<string>
                {
                    Success = true,
                    Data = demandeId.ToString(),
                    Message = $"Demande de réactivation créée pour {inactivePatient.Prenom} {inactivePatient.Nom}. Un email de vérification a été envoyé à {email}."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la demande de réactivation pour {Email}", email);
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la demande de réactivation"
                };
            }
        }

        public async Task<ApiResponse<Patient>> ConfirmReactivationAsync(string token)
        {
            try
            {
                var success = await _patientRepository.ConfirmReactivationByTokenAsync(token);
                if (!success)
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Token de réactivation invalide ou expiré"
                    };
                }

                // TODO: Récupérer les infos du patient réactivé pour l'email
                // ENVOYER EMAIL DE CONFIRMATION - AJOUTEZ CES LIGNES
                // await _emailService.SendReactivationConfirmationAsync(email, patientName);

                return new ApiResponse<Patient>
                {
                    Success = true,
                    Message = "Patient réactivé avec succès ! Vous pouvez maintenant vous connecter."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la confirmation de réactivation");
                return new ApiResponse<Patient>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la réactivation"
                };
            }
        }
        //public async Task<ApiResponse<Patient>> ConfirmReactivationAsync(string token)
        //{
        //    try
        //    {
        //        var success = await _patientRepository.ConfirmReactivationByTokenAsync(token);
        //        if (!success)
        //        {
        //            return new ApiResponse<Patient>
        //            {
        //                Success = false,
        //                Message = "Token de réactivation invalide ou expiré"
        //            };
        //        }

        //        return new ApiResponse<Patient>
        //        {
        //            Success = true,
        //            Message = "Patient réactivé avec succès ! Vous pouvez maintenant vous connecter."
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Erreur lors de la confirmation de réactivation");
        //        return new ApiResponse<Patient>
        //        {
        //            Success = false,
        //            Message = "Une erreur est survenue lors de la réactivation"
        //        };
        //    }
        //}

        // Recherche patient inactif par email
        public async Task<ApiResponse<Patient>> FindInactivePatientByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Email requis pour la recherche"
                    };
                }

                var inactivePatient = await _patientRepository.FindInactivePatientByEmailAsync(email);

                if (inactivePatient == null)
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Aucun patient inactif trouvé avec cet email"
                    };
                }

                return new ApiResponse<Patient>
                {
                    Success = true,
                    Data = inactivePatient,
                    Message = $"Patient inactif trouvé : {inactivePatient.Prenom} {inactivePatient.Nom}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche du patient inactif par email {Email}", email);
                return new ApiResponse<Patient>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la recherche par email"
                };
            }
        }

        // Recherche patient inactif par téléphone
        public async Task<ApiResponse<Patient>> FindInactivePatientByTelephoneAsync(string telephone)
        {
            try
            {
                if (string.IsNullOrEmpty(telephone))
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Téléphone requis pour la recherche"
                    };
                }

                var inactivePatient = await _patientRepository.FindInactivePatientByTelephoneAsync(telephone);

                if (inactivePatient == null)
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Aucun patient inactif trouvé avec ce téléphone"
                    };
                }

                return new ApiResponse<Patient>
                {
                    Success = true,
                    Data = inactivePatient,
                    Message = $"Patient inactif trouvé : {inactivePatient.Prenom} {inactivePatient.Nom}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche du patient inactif par téléphone {Telephone}", telephone);
                return new ApiResponse<Patient>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la recherche par téléphone"
                };
            }
        }

        // Recherche patient inactif par numéro d'assurance
        public async Task<ApiResponse<Patient>> FindInactivePatientByNumeroAssuranceAsync(string numeroAssurance)
        {
            try
            {
                if (string.IsNullOrEmpty(numeroAssurance))
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Numéro d'assurance requis pour la recherche"
                    };
                }

                var inactivePatient = await _patientRepository.FindInactivePatientByNumeroAssuranceAsync(numeroAssurance);

                if (inactivePatient == null)
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Aucun patient inactif trouvé avec ce numéro d'assurance"
                    };
                }

                return new ApiResponse<Patient>
                {
                    Success = true,
                    Data = inactivePatient,
                    Message = $"Patient inactif trouvé : {inactivePatient.Prenom} {inactivePatient.Nom}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche du patient inactif par numéro d'assurance {NumeroAssurance}", numeroAssurance);
                return new ApiResponse<Patient>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la recherche par numéro d'assurance"
                };
            }
        }

        // Recherche patient inactif par UserId
        public async Task<ApiResponse<Patient>> FindInactivePatientByUserIdAsync(Guid userId)
        {
            try
            {
                var inactivePatient = await _patientRepository.FindInactivePatientByUserIdAsync(userId);

                if (inactivePatient == null)
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Aucun patient inactif trouvé avec cet UserId"
                    };
                }

                return new ApiResponse<Patient>
                {
                    Success = true,
                    Data = inactivePatient,
                    Message = $"Patient inactif trouvé : {inactivePatient.Prenom} {inactivePatient.Nom}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche du patient inactif par UserId {UserId}", userId);
                return new ApiResponse<Patient>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la recherche par UserId"
                };
            }
        }

        // Recherche patient inactif par nom complet
        public async Task<ApiResponse<Patient>> FindInactivePatientByNomCompletAsync(string nom, string prenom)
        {
            try
            {
                if (string.IsNullOrEmpty(nom) || string.IsNullOrEmpty(prenom))
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Nom et prénom requis pour la recherche"
                    };
                }

                var inactivePatient = await _patientRepository.FindInactivePatientByNomCompletAsync(nom, prenom);

                if (inactivePatient == null)
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Aucun patient inactif trouvé avec ce nom complet"
                    };
                }

                return new ApiResponse<Patient>
                {
                    Success = true,
                    Data = inactivePatient,
                    Message = $"Patient inactif trouvé : {inactivePatient.Prenom} {inactivePatient.Nom}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche du patient inactif par nom complet {Nom} {Prenom}", nom, prenom);
                return new ApiResponse<Patient>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la recherche par nom complet"
                };
            }
        }

        public async Task<ApiResponse<Patient>> CreatePatientWithPhotoAsync(CreatePatientWithPhotoDto dto)
        {
            try
            {
                // Vérifier format image
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(dto.Photo.ContentType.ToLower()))
                {
                    return new ApiResponse<Patient> { Success = false, Message = "Type de fichier non autorisé" };
                }

                // Sauvegarde physique
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Photo.FileName)}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "patients");
                Directory.CreateDirectory(folderPath);
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Photo.CopyToAsync(stream);
                }

                var photoUrl = $"/uploads/patients/{fileName}";

                // Construire CreatePatientDto
                var createDto = new CreatePatientDto
                {
                    NumeroAssuranceMaladie = dto.NumeroAssuranceMaladie,
                    Nom = dto.Nom,
                    Prenom = dto.Prenom,
                    DateNaissance = dto.DateNaissance,
                    Sexe = dto.Sexe,
                    Telephone = dto.Telephone,
                    Email = dto.Email,
                    Adresse = dto.Adresse,
                    CodePostal = dto.CodePostal,
                    Ville = dto.Ville,
                    GroupeSanguin = dto.GroupeSanguin,
                    UserId = dto.UserId,
                    PhotoUrl = photoUrl,
                    PhotoNom = dto.Photo.FileName,
                    PhotoType = dto.Photo.ContentType,
                    PhotoTaille = dto.Photo.Length
                };

                return await CreatePatientAsync(createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création d'un patient avec photo");
                return new ApiResponse<Patient> { Success = false, Message = "Erreur interne serveur" };
            }
        }



    }

}
