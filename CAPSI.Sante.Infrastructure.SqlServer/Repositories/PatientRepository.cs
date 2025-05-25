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
                        patient.GroupeSanguin
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
                    "SELECT * FROM Patients WHERE NumeroAssuranceMaladie = @NumeroAssurance",
                    new { NumeroAssurance = numeroAssurance }
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
                    "SELECT * FROM Patients ORDER BY Nom, Prenom"
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
                return await conn.QueryAsync<Patient>(@"
                SELECT * FROM Patients 
                WHERE Nom LIKE @Search 
                OR Prenom LIKE @Search 
                OR NumeroAssuranceMaladie LIKE @Search
                OR Telephone LIKE @Search
                OR Email LIKE @Search
                ORDER BY Nom, Prenom",
                    new { Search = $"%{searchTerm}%" }
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
                var query = "SELECT COUNT(1) FROM Patients WHERE NumeroAssuranceMaladie = @NumeroAssurance";
                var parameters = new DynamicParameters();
                parameters.Add("NumeroAssurance", numeroAssurance);

                if (excludeId.HasValue)
                {
                    query += " AND Id != @ExcludeId";
                    parameters.Add("ExcludeId", excludeId.Value);
                }

                var count = await conn.ExecuteScalarAsync<int>(query, parameters);
                return count == 0;
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

                // Récupérer le patient
                var patient = await GetByIdAsync(id);

                if (patient != null)
                {
                    // Récupérer le dossier médical associé
                    var dossier = await conn.QueryFirstOrDefaultAsync<DossierMedical>(
                        "SELECT * FROM DossiersMedicaux WHERE PatientId = @PatientId",
                        new { PatientId = id }
                    );

                    patient.DossierMedical = dossier;
                }

                return patient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du patient avec dossier {PatientId}", id);
                throw;
            }
        }
    }
}
