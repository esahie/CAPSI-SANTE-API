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

        public PatientService(IPatientRepository patientRepository, ILogger<PatientService> logger)
        {
            _patientRepository = patientRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<Patient>> CreatePatientAsync(CreatePatientDto dto)
        {
            try
            {
                // Validation
                var validation = ValidatePatient(dto);
                if (!validation.IsValid)
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Errors = validation.Errors
                    };
                }

                // Vérifier si le numéro d'assurance existe déjà
                var existingPatient = await _patientRepository.GetByNumeroAssuranceAsync(dto.NumeroAssuranceMaladie);
                if (existingPatient != null)
                {
                    return new ApiResponse<Patient>
                    {
                        Success = false,
                        Message = "Ce numéro d'assurance maladie existe déjà"
                    };
                }

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

                // Vérifier si le nouveau numéro d'assurance n'existe pas déjà pour un autre patient
                if (dto.NumeroAssuranceMaladie != existingPatient.NumeroAssuranceMaladie)
                {
                    var patientWithSameNumber = await _patientRepository.GetByNumeroAssuranceAsync(dto.NumeroAssuranceMaladie);
                    if (patientWithSameNumber != null && patientWithSameNumber.Id != dto.Id)
                    {
                        return new ApiResponse<Patient>
                        {
                            Success = false,
                            Message = "Ce numéro d'assurance maladie est déjà utilisé"
                        };
                    }
                }
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

            // Validation du numéro d'assurance
            //if (string.IsNullOrEmpty(dto.NumeroAssuranceMaladie))
            //    result.Errors.Add("Le numéro d'assurance maladie est requis");
            //else if (!Regex.IsMatch(dto.NumeroAssuranceMaladie, @"^[A-Z]{4}\d{8}$"))
            //    result.Errors.Add("Format du numéro d'assurance maladie invalide");

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
    }

}
