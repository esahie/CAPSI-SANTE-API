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
    public class AntecedentMedicalRepository : IAntecedentMedicalRepository
    {
        private readonly IDatabaseConnection _connection;
        private readonly ILogger<AntecedentMedicalRepository> _logger;

        public AntecedentMedicalRepository(
            IDatabaseConnection connection,
            ILogger<AntecedentMedicalRepository> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task<AntecedentMedical> AddAsync(AntecedentMedical antecedent)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                antecedent.Id = await conn.QuerySingleAsync<Guid>(
                    "sp_InsertAntecedentMedical",
                    new
                    {
                        antecedent.DossierId,
                        antecedent.Type,
                        antecedent.Description,
                        antecedent.DateDiagnostic
                    },
                    commandType: CommandType.StoredProcedure
                );
                return antecedent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout de l'antécédent médical");
                throw;
            }
        }

        public async Task<AntecedentMedical> GetByIdAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<AntecedentMedical>(
                    "SELECT * FROM AntecedentsMedicaux WHERE AntecedentId = @Id",
                    new { Id = id }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'antécédent médical {AntecedentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<AntecedentMedical>> GetByDossierIdAsync(Guid dossierId)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryAsync<AntecedentMedical>(
                    "SELECT * FROM AntecedentsMedicaux WHERE DossierId = @DossierId ORDER BY DateDiagnostic DESC",
                    new { DossierId = dossierId }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des antécédents du dossier {DossierId}", dossierId);
                throw;
            }
        }

        public async Task<IEnumerable<AntecedentMedical>> GetByTypeAsync(Guid dossierId, string type)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryAsync<AntecedentMedical>(
                    "SELECT * FROM AntecedentsMedicaux WHERE DossierId = @DossierId AND Type = @Type ORDER BY DateDiagnostic DESC",
                    new { DossierId = dossierId, Type = type }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des antécédents de type {Type} du dossier {DossierId}", type, dossierId);
                throw;
            }
        }

        public async Task<AntecedentMedical> UpdateAsync(AntecedentMedical antecedent)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var result = await conn.ExecuteAsync(
                    "UPDATE AntecedentsMedicaux SET Type = @Type, Description = @Description, DateDiagnostic = @DateDiagnostic WHERE AntecedentId = @Id",
                    new
                    {
                        Id = antecedent.Id,
                        antecedent.Type,
                        antecedent.Description,
                        antecedent.DateDiagnostic
                    }
                );

                if (result == 0)
                    throw new NotFoundException($"Antécédent médical avec ID {antecedent.Id} non trouvé");

                return await GetByIdAsync(antecedent.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'antécédent médical {AntecedentId}", antecedent.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var result = await conn.ExecuteAsync(
                    "DELETE FROM AntecedentsMedicaux WHERE AntecedentId = @Id",
                    new { Id = id }
                );

                if (result == 0)
                    throw new NotFoundException($"Antécédent médical avec ID {id} non trouvé");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'antécédent médical {AntecedentId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.ExecuteScalarAsync<bool>(
                    "SELECT COUNT(1) FROM AntecedentsMedicaux WHERE AntecedentId = @Id",
                    new { Id = id }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'existence de l'antécédent médical {AntecedentId}", id);
                throw;
            }
        }
    }
}
