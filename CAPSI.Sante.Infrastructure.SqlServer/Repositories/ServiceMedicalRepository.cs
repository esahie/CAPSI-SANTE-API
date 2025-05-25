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
using System.Threading.Tasks;

namespace CAPSI.Sante.Infrastructure.SqlServer.Repositories
{
    public class ServiceMedicalRepository : IServiceMedicalRepository
    {
        private readonly IDatabaseConnection _connection;
        private readonly ILogger<ServiceMedicalRepository> _logger;

        public ServiceMedicalRepository(
        IDatabaseConnection connection,
        ILogger<ServiceMedicalRepository> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        // CREATE
        public async Task<ServiceMedical> AddAsync(ServiceMedical entity)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var id = await conn.QuerySingleAsync<Guid>(
                    "sp_InsertServiceMedical",
                    new
                    {
                        entity.Code,
                        entity.Nom,
                        entity.Description,
                        entity.DureeParDefaut,
                        entity.Tarif,
                        entity.RequiertAssurance,
                        entity.EstActif
                    },
                    commandType: CommandType.StoredProcedure
                );

                entity.Id = id;
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout du service médical");
                throw;
            }
        }

        // READ
        public async Task<ServiceMedical> GetByIdAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<ServiceMedical>(
                    "sp_GetServiceMedicalById",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du service {ServiceId}", id);
                throw;
            }
        }

        public async Task<ServiceMedical> GetByCodeAsync(string code)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<ServiceMedical>(
                    "sp_GetServiceMedicalByCode",
                    new { Code = code },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du service par code {Code}", code);
                throw;
            }
        }

        public async Task<IEnumerable<ServiceMedical>> GetAllAsync(bool includeInactive = false)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryAsync<ServiceMedical>(
                    "sp_GetAllServicesMedicaux",
                    new { IncludeInactive = includeInactive },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de tous les services");
                throw;
            }
        }

        public async Task<IEnumerable<ServiceMedical>> GetByMedecinAsync(Guid medecinId, bool includeInactive = false)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryAsync<ServiceMedical>(
                    "sp_GetServicesMedicauxByMedecin",
                    new
                    {
                        MedecinId = medecinId,
                        IncludeInactive = includeInactive
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des services du médecin {MedecinId}", medecinId);
                throw;
            }
        }

        // UPDATE
        public async Task<ServiceMedical> UpdateAsync(ServiceMedical entity)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var result = await conn.ExecuteAsync(
                    "sp_UpdateServiceMedical",
                    new
                    {
                        entity.Id,
                        entity.Code,
                        entity.Nom,
                        entity.Description,
                        entity.DureeParDefaut,
                        entity.Tarif,
                        entity.RequiertAssurance,
                        entity.EstActif,
                        UpdatedAt = DateTime.UtcNow
                    },
                    commandType: CommandType.StoredProcedure
                );

                if (result == 0)
                    throw new NotFoundException($"Service avec ID {entity.Id} non trouvé");

                // Récupérer le service mis à jour
                return await GetByIdAsync(entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du service {ServiceId}", entity.Id);
                throw;
            }
        }

        // ACTIVATE/DEACTIVATE
        public async Task<ServiceMedical> ActivateAsync(Guid id)
        {
            try
            {
                var service = await GetByIdAsync(id);
                if (service == null)
                    throw new NotFoundException($"Service avec ID {id} non trouvé");

                service.EstActif = true;
                service.UpdatedAt = DateTime.UtcNow;

                using var conn = _connection.CreateConnection();
                await conn.ExecuteAsync(
                    "sp_ChangeServiceActivationStatus",
                    new
                    {
                        Id = id,
                        EstActif = true,
                        UpdatedAt = DateTime.UtcNow
                    },
                    commandType: CommandType.StoredProcedure
                );

                return await GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'activation du service {ServiceId}", id);
                throw;
            }
        }

        public async Task<ServiceMedical> DeactivateAsync(Guid id)
        {
            try
            {
                var service = await GetByIdAsync(id);
                if (service == null)
                    throw new NotFoundException($"Service avec ID {id} non trouvé");

                service.EstActif = false;
                service.UpdatedAt = DateTime.UtcNow;

                using var conn = _connection.CreateConnection();
                await conn.ExecuteAsync(
                    "sp_ChangeServiceActivationStatus",
                    new
                    {
                        Id = id,
                        EstActif = false,
                        UpdatedAt = DateTime.UtcNow
                    },
                    commandType: CommandType.StoredProcedure
                );

                return await GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la désactivation du service {ServiceId}", id);
                throw;
            }
        }

        // DELETE (suppression définitive)
        public async Task DeleteAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var result = await conn.ExecuteAsync(
                    "sp_DeleteServiceMedical",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );

                if (result == 0)
                    throw new NotFoundException($"Service avec ID {id} non trouvé");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du service {ServiceId}", id);
                throw;
            }
        }

        public async Task<bool> IsUsedInAppointmentsAsync(Guid serviceId)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var count = await conn.ExecuteScalarAsync<int>(
                    "sp_IsServiceUsedInAppointments",
                    new { ServiceId = serviceId },
                    commandType: CommandType.StoredProcedure
                );

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'utilisation du service {ServiceId}", serviceId);
                throw;
            }
        }

        // Méthodes utilitaires
        public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var count = await conn.ExecuteScalarAsync<int>(
                    "sp_IsServiceCodeUnique",
                    new
                    {
                        Code = code,
                        ExcludeId = excludeId
                    },
                    commandType: CommandType.StoredProcedure
                );

                return count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'unicité du code {Code}", code);
                throw;
            }
        }
    }
}