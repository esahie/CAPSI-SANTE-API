using CAPSI.Sante.Application.Data;
using CAPSI.Sante.Application.Repositories.SQLserver.Interfaces; 
using CAPSI.Sante.Domain.Exceptions;
using CAPSI.Sante.Domain.Models.SQLserver;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Infrastructure.SqlServer.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly IDatabaseConnection _connection;
        private readonly ILogger<PatientRepository> _logger;

        public PatientRepository(
            IDatabaseConnection connection,
            ILogger<PatientRepository> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task<Patient> AddAsync(Patient patient)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                patient.Id = await conn.QuerySingleAsync<Guid>(
                    "sp_InsertPatient",
                    new
                    {
                        patient.UserId,
                        patient.NumeroAssuranceMaladie,
                        patient.Nom,
                        patient.Prenom,
                        patient.DateNaissance,
                        patient.Sexe,
                        patient.Telephone,
                        patient.Email,
                        patient.Adresse,
                        patient.CodePostal,
                        patient.Ville,
                        patient.GroupeSanguin,
                        patient.PhotoUrl // Nouvelle propriété
                    },
                    commandType: CommandType.StoredProcedure
                );
                return patient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout du patient");
                throw;
            }
        }

        

        // READ
        public async Task<Patient> GetByIdAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<Patient>(
                    "sp_GetPatientById",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du patient {PatientId}", id);
                throw;
            }
        }

        public async Task<Patient> GetByNumeroAssuranceAsync(string numeroAssurance)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<Patient>(
                    "sp_GetPatientByNumeroAssurance",
                    new { NumeroAssurance = numeroAssurance },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du patient par numéro d'assurance {NumeroAssurance}", numeroAssurance);
                throw;
            }
        }


        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryAsync<Patient>(
                    "sp_GetPatients",
                    new { IncludeInactive = false },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de tous les patients");
                throw;
            }
        }

        // UPDATE

        public async Task<Patient> UpdateAsync(Patient patient)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var rowsAffected = await conn.ExecuteAsync(
                    "sp_UpdatePatient",
                    new
                    {
                        patient.Id,
                        patient.NumeroAssuranceMaladie,
                        patient.Nom,
                        patient.Prenom,
                        patient.DateNaissance,
                        patient.Sexe,
                        patient.Telephone,
                        patient.Email,
                        patient.Adresse,
                        patient.CodePostal,
                        patient.Ville,
                        patient.GroupeSanguin,
                        patient.PhotoUrl, // Nouvelle propriété
                        UpdatedAt = DateTime.UtcNow
                    },
                    commandType: CommandType.StoredProcedure
                );

                if (rowsAffected == 0)
                    throw new NotFoundException($"Patient avec ID {patient.Id} non trouvé");

                // Récupérer le patient mis à jour
                var updatedPatient = await GetByIdAsync(patient.Id);
                if (updatedPatient == null)
                    throw new NotFoundException($"Patient avec ID {patient.Id} non trouvé après mise à jour");

                return updatedPatient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du patient {PatientId}", patient.Id);
                throw;
            }
        }
         
        // DELETE
        public async Task DeleteAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var rowsAffected = await conn.ExecuteAsync(
                    "sp_DeletePatient",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );

                if (rowsAffected == 0)
                    throw new NotFoundException($"Patient avec ID {id} non trouvé");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du patient {PatientId}", id);
                throw;
            }
        }

        // Méthodes de recherche supplémentaires
        public async Task<IEnumerable<Patient>> SearchAsync(string searchTerm)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryAsync<Patient>(
                    "sp_SearchPatients",
                    new { SearchTerm = searchTerm },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche de patients avec le terme {SearchTerm}", searchTerm);
                throw;
            }
        }

        // Méthodes utilitaires supplémentaires
        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.ExecuteScalarAsync<bool>(
                    "sp_CheckPatientExists",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'existence du patient {PatientId}", id);
                throw;
            }
        }


        public async Task<bool> IsNumeroAssuranceUnique(string numeroAssurance, Guid? excludeId = null)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var result = await conn.QuerySingleAsync<int>(
                    "sp_CheckNumeroAssuranceUnique",
                    new
                    {
                        NumeroAssurance = numeroAssurance,
                        ExcludeId = excludeId
                    },
                    commandType: CommandType.StoredProcedure
                );
                return result == 0; // 0 = unique, >0 = existe déjà
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'unicité du numéro d'assurance {NumeroAssurance}", numeroAssurance);
                throw;
            }
        }

        public async Task<bool> DeactivateAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var rowsAffected = await conn.ExecuteScalarAsync<int>(
                    "sp_DeactivatePatient",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );

                if (rowsAffected == 0)
                    throw new NotFoundException($"Patient avec ID {id} non trouvé");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la désactivation du patient {PatientId}", id);
                throw;
            }
        }

        public async Task<bool> ReactivateAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var rowsAffected = await conn.ExecuteScalarAsync<int>(
                    "sp_ReactivatePatient",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );

                if (rowsAffected == 0)
                    throw new NotFoundException($"Patient avec ID {id} non trouvé");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la réactivation du patient {PatientId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Patient>> GetAllAsync(bool includeInactive = false)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryAsync<Patient>(
                    "sp_GetPatients",
                    new { IncludeInactive = includeInactive },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de tous les patients");
                throw;
            }
        }

        public async Task<IEnumerable<Patient>> SearchAsync(string searchTerm, bool includeInactive = false)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryAsync<Patient>(
                    "sp_RechercherPatients",
                    new
                    {
                        Recherche = searchTerm,
                        IncludeInactive = includeInactive,
                        PageNumber = 1,
                        PageSize = 1000 // Valeur élevée pour récupérer tous les résultats
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche de patients");
                throw;
            }
        }


        public async Task<Patient> GetByIdWithDossierAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<Patient>(
                    "sp_GetPatientWithDossier",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du patient avec dossier {PatientId}", id);
                throw;
            }
        }
        public async Task<bool> UpdatePhotoAsync(Guid patientId, string photoUrl)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var rowsAffected = await conn.ExecuteAsync(
                    "sp_UpdatePatientPhoto",
                    new { PatientId = patientId, PhotoUrl = photoUrl },
                    commandType: CommandType.StoredProcedure
                );

                if (rowsAffected == 0)
                    throw new NotFoundException($"Patient avec ID {patientId} non trouvé");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la photo du patient {PatientId}", patientId);
                throw;
            }
        }

        // Méthode pour supprimer la photo d'un patient
        public async Task<bool> DeletePhotoAsync(Guid patientId)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var rowsAffected = await conn.ExecuteAsync(
                    "sp_DeletePatientPhoto",
                    new { PatientId = patientId },
                    commandType: CommandType.StoredProcedure
                );

                if (rowsAffected == 0)
                    throw new NotFoundException($"Patient avec ID {patientId} non trouvé");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la photo du patient {PatientId}", patientId);
                throw;
            }
        }

        // Méthode pour récupérer uniquement l'URL de la photo
        public async Task<string> GetPhotoUrlAsync(Guid patientId)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<string>(
                    "sp_GetPatientPhotoUrl",
                    new { PatientId = patientId },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la photo du patient {PatientId}", patientId);
                throw;
            }
        }

        // Méthode pour vérifier unicité email
        public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return true; // Email vide est considéré comme unique

                using var conn = _connection.CreateConnection();
                var count = await conn.QuerySingleAsync<int>(
                    "sp_CheckEmailUnique",
                    new
                    {
                        Email = email,
                        ExcludeId = excludeId
                    },
                    commandType: CommandType.StoredProcedure
                );
                return count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'unicité de l'email {Email}", email);
                throw;
            }
        }

        // Méthode pour vérifier unicité téléphone
        public async Task<bool> IsTelephoneUniqueAsync(string telephone, Guid? excludeId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(telephone))
                    return true; // Téléphone vide est considéré comme unique

                using var conn = _connection.CreateConnection();
                var count = await conn.QuerySingleAsync<int>(
                    "sp_CheckTelephoneUnique",
                    new
                    {
                        Telephone = telephone,
                        ExcludeId = excludeId
                    },
                    commandType: CommandType.StoredProcedure
                );
                return count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'unicité du téléphone {Telephone}", telephone);
                throw;
            }
        }

        // Méthode pour vérifier unicité UserId
        public async Task<bool> IsUserIdUniqueAsync(Guid? userId, Guid? excludeId = null)
        {
            try
            {
                if (!userId.HasValue)
                    return true; // UserId null est considéré comme unique

                using var conn = _connection.CreateConnection();
                var count = await conn.QuerySingleAsync<int>(
                    "sp_CheckUserIdUnique",
                    new
                    {
                        UserId = userId.Value,
                        ExcludeId = excludeId
                    },
                    commandType: CommandType.StoredProcedure
                );
                return count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'unicité du UserId {UserId}", userId);
                throw;
            }
        }

        // Méthode pour récupérer patient par email
        public async Task<Patient> GetByEmailAsync(string email)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<Patient>(
                    "sp_GetPatientByEmail",
                    new { Email = email },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du patient par email {Email}", email);
                throw;
            }
        }

        // Méthode pour récupérer patient par téléphone
        public async Task<Patient> GetByTelephoneAsync(string telephone)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<Patient>(
                    "sp_GetPatientByTelephone",
                    new { Telephone = telephone },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du patient par téléphone {Telephone}", telephone);
                throw;
            }
        }

        // Méthode pour récupérer patient par UserId
        public async Task<Patient> GetByUserIdAsync(Guid userId)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<Patient>(
                    "sp_GetPatientByUserId",
                    new { UserId = userId },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du patient par UserId {UserId}", userId);
                throw;
            }
        }

        // Trouver patient inactif par email
        public async Task<Patient> FindInactivePatientByEmailAsync(string email)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<Patient>(
                    "sp_FindInactivePatientByEmail",
                    new { Email = email },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche du patient inactif par email {Email}", email);
                throw;
            }
        }

        // Trouver patient inactif par téléphone
        public async Task<Patient> FindInactivePatientByTelephoneAsync(string telephone)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<Patient>(
                    "sp_FindInactivePatientByTelephone",
                    new { Telephone = telephone },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche du patient inactif par téléphone {Telephone}", telephone);
                throw;
            }
        }

        // Trouver patient inactif par UserId
        public async Task<Patient> FindInactivePatientByUserIdAsync(Guid userId)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<Patient>(
                    "sp_FindInactivePatientByUserId",
                    new { UserId = userId },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche du patient inactif par UserId {UserId}", userId);
                throw;
            }
        }

        // Trouver patient inactif par numéro d'assurance
        public async Task<Patient> FindInactivePatientByNumeroAssuranceAsync(string numeroAssurance)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<Patient>(
                    "sp_FindInactivePatientByNumeroAssurance",
                    new { NumeroAssurance = numeroAssurance },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche du patient inactif par numéro d'assurance {NumeroAssurance}", numeroAssurance);
                throw;
            }
        }

        // Trouver patient inactif par nom complet
        public async Task<Patient> FindInactivePatientByNomCompletAsync(string nom, string prenom)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<Patient>(
                    "sp_FindInactivePatientByNomComplet",
                    new { Nom = nom, Prenom = prenom },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche du patient inactif par nom complet {Nom} {Prenom}", nom, prenom);
                throw;
            }
        }

        // Créer demande de réactivation
        public async Task<Guid> CreateDemandeReactivationAsync(Guid patientId, string emailDemandeur, string motifDemande = null)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var token = Guid.NewGuid().ToString("N"); // Token de vérification

                var demandeId = await conn.QuerySingleAsync<Guid>(
                    "sp_CreateDemandeReactivation",
                    new
                    {
                        PatientId = patientId,
                        EmailDemandeur = emailDemandeur,
                        MotifDemande = motifDemande,
                        TokenVerification = token
                    },
                    commandType: CommandType.StoredProcedure
                );

                return demandeId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la demande de réactivation pour le patient {PatientId}", patientId);
                throw;
            }
        }

        // Confirmer réactivation par token
        public async Task<bool> ConfirmReactivationByTokenAsync(string token)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var result = await conn.QuerySingleAsync<dynamic>(
                    "sp_ConfirmReactivationByToken",
                    new { Token = token },
                    commandType: CommandType.StoredProcedure
                );

                return result.Success == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la confirmation de réactivation par token");
                throw;
            }
        }

    }
}
