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
    public class DossierMedicalRepository : IDossierMedicalRepository
    {
        private readonly IDatabaseConnection _connection;
        private readonly ILogger<DossierMedicalRepository> _logger;

        public DossierMedicalRepository(
            IDatabaseConnection connection,
            ILogger<DossierMedicalRepository> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task<DossierMedical> AddAsync(DossierMedical dossier)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                dossier.Id = await conn.QuerySingleAsync<Guid>(
                    "sp_InsertDossierMedical",
                    new
                    {
                        dossier.PatientId,
                        DateCreation = DateTime.UtcNow
                    },
                    commandType: CommandType.StoredProcedure
                );
                return dossier;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout du dossier médical");
                throw;
            }
        }

        public async Task<DossierMedical> GetByIdAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<DossierMedical>(
                    "sp_GetDossierMedicalById",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du dossier médical {DossierId}", id);
                throw;
            }
        }

        public async Task<DossierMedical> GetByPatientIdAsync(Guid patientId)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<DossierMedical>(
                    "SELECT * FROM DossiersMedicaux WHERE PatientId = @PatientId",
                    new { PatientId = patientId }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du dossier médical par patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<DossierMedical>> GetAllAsync()
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryAsync<DossierMedical>(
                    "SELECT * FROM DossiersMedicaux ORDER BY DateCreation DESC"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de tous les dossiers médicaux");
                throw;
            }
        }

        public async Task<DossierMedical> UpdateAsync(DossierMedical dossier)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                await conn.ExecuteAsync(
                    "UPDATE DossiersMedicaux SET PatientId = @PatientId, DerniereMiseAJour = @DerniereMiseAJour WHERE DossierId = @Id",
                    new
                    {
                        Id = dossier.Id,
                        dossier.PatientId,
                        DerniereMiseAJour = DateTime.UtcNow
                    }
                );

                return await GetByIdAsync(dossier.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du dossier médical {DossierId}", dossier.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var result = await conn.ExecuteAsync(
                    "DELETE FROM DossiersMedicaux WHERE DossierId = @Id",
                    new { Id = id }
                );

                if (result == 0)
                    throw new NotFoundException($"Dossier médical avec ID {id} non trouvé");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du dossier médical {DossierId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.ExecuteScalarAsync<bool>(
                    "SELECT COUNT(1) FROM DossiersMedicaux WHERE DossierId = @Id",
                    new { Id = id }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'existence du dossier médical {DossierId}", id);
                throw;
            }
        }
    }
}
