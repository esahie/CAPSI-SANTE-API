using CAPSI.Sante.Application.Data;
using CAPSI.Sante.Application.Repositories.SQLserver.Interfaces;
using CAPSI.Sante.Domain.Exceptions;
using CAPSI.Sante.Domain.Models.SQLserver;
using CAPSI.Sante.Infrastructure.SqlServer.Data;
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
    public class MedecinRepository : IMedecinRepository
    {
        private readonly IDatabaseConnection _connection;
        private readonly ILogger<MedecinRepository> _logger;

        public MedecinRepository(IDatabaseConnection connection,
        ILogger<MedecinRepository> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        private async Task<T> ExecuteTransactionAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> action)
        {
            using var conn = _connection.CreateConnection();
            try
            {
                conn.Open(); // Ouvrir explicitement la connexion
                using var transaction = conn.BeginTransaction();

                try
                {
                    var result = await action(conn, transaction);
                    transaction.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Erreur lors de l'exécution de la transaction");
                    throw;
                }
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        public async Task<Medecin> AddAsync(Medecin medecin)
        {
            return await ExecuteTransactionAsync(async (conn, transaction) =>
            {
                // 1. Insérer le médecin avec procédure stockée
                medecin.Id = await conn.QuerySingleAsync<Guid>(
                    "sp_InsertMedecin",
                    new
                    {
                        medecin.NumeroLicence,
                        medecin.UserId,
                        medecin.Nom,
                        medecin.Prenom,
                        medecin.Specialite,
                        medecin.Telephone,
                        medecin.Email,
                        medecin.AdresseCabinet,
                        medecin.CodePostal,
                        medecin.Ville,
                        medecin.PhotoUrl,
                        medecin.PhotoNom,
                        medecin.PhotoType,
                        medecin.PhotoTaille
                    },
                    transaction,
                    commandType: CommandType.StoredProcedure
                );

                // 2. Associer les services médicaux via procédure stockée
                if (medecin.ServicesOfferts?.Any() == true)
                {
                    foreach (var service in medecin.ServicesOfferts)
                    {
                        await conn.ExecuteAsync(
                            "sp_InsertMedecinService",
                            new { MedecinId = medecin.Id, ServiceId = service.Id },
                            transaction,
                            commandType: CommandType.StoredProcedure
                        );
                    }
                }

                return medecin;
            });
        }

        // READ
        public async Task<Medecin> GetByIdAsync(Guid id)
        {
            using var conn = _connection.CreateConnection();
            try
            {
                conn.Open();

                // Récupérer le médecin avec procédure stockée
                var medecin = await conn.QueryFirstOrDefaultAsync<Medecin>(
                    "sp_GetMedecinById",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );

                if (medecin != null)
                {
                    // Récupérer les services associés avec procédure stockée
                    var services = await conn.QueryAsync<ServiceMedical>(
                        "sp_GetServicesByMedecinId",
                        new { MedecinId = id },
                        commandType: CommandType.StoredProcedure
                    );
                    medecin.ServicesOfferts = services.ToList();
                }

                return medecin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du médecin {MedecinId}", id);
                throw;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        public async Task<Medecin> GetByLicenceAsync(string numeroLicence)
        {
            using var conn = _connection.CreateConnection();
            try
            {
                conn.Open();

                // Utiliser la procédure stockée
                var medecin = await conn.QueryFirstOrDefaultAsync<Medecin>(
                    "sp_GetMedecinByLicence",
                    new { NumeroLicence = numeroLicence },
                    commandType: CommandType.StoredProcedure
                );

                if (medecin != null)
                {
                    // Récupérer les services associés avec procédure stockée
                    var services = await conn.QueryAsync<ServiceMedical>(
                        "sp_GetServicesByMedecinId",
                        new { MedecinId = medecin.Id },
                        commandType: CommandType.StoredProcedure
                    );
                    medecin.ServicesOfferts = services.ToList();
                }

                return await GetByLicenceAsync(numeroLicence, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du médecin par licence {NumeroLicence}", numeroLicence);
                throw;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        // Dans MedecinRepository.cs, ajoutez une surcharge à GetByLicenceAsync
        public async Task<Medecin> GetByLicenceAsync(string numeroLicence, IDbConnection existingConnection = null)
        {
            bool ownsConnection = existingConnection == null;
            var conn = ownsConnection ? _connection.CreateConnection() : existingConnection;

            try
            {
                if (ownsConnection)
                {
                    conn.Open();
                }

                // Utiliser la procédure stockée
                var medecin = await conn.QueryFirstOrDefaultAsync<Medecin>(
                    "sp_GetMedecinByLicence",
                    new { NumeroLicence = numeroLicence },
                    commandType: CommandType.StoredProcedure
                );

                if (medecin != null)
                {
                    // Récupérer les services associés avec procédure stockée
                    var services = await conn.QueryAsync<ServiceMedical>(
                        "sp_GetServicesByMedecinId",
                        new { MedecinId = medecin.Id },
                        commandType: CommandType.StoredProcedure
                    );
                    medecin.ServicesOfferts = services.ToList();
                }

                return medecin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du médecin par licence {NumeroLicence}", numeroLicence);
                throw;
            }
            finally
            {
                if (ownsConnection && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        // De même, ajoutez une surcharge à AddAsync
        public async Task<Medecin> AddAsync(Medecin medecin, IDbConnection existingConnection = null)
        {
            if (existingConnection != null)
            {
                // Si une connexion existante est fournie, utilisez-la dans une transaction
                using var transaction = existingConnection.BeginTransaction();
                try
                {
                    // 1. Insérer le médecin avec procédure stockée
                    medecin.Id = await existingConnection.QuerySingleAsync<Guid>(
                        "sp_InsertMedecin",
                        new
                        {
                            medecin.NumeroLicence,
                            medecin.UserId,
                            medecin.Nom,
                            medecin.Prenom,
                            medecin.Specialite,
                            medecin.Telephone,
                            medecin.Email,
                            medecin.AdresseCabinet,
                            medecin.CodePostal,
                            medecin.Ville,
                            medecin.PhotoUrl,
                            medecin.PhotoNom,
                            medecin.PhotoType,
                            medecin.PhotoTaille
                        },
                        transaction,
                        commandType: CommandType.StoredProcedure
                    );

                    // 2. Associer les services médicaux via procédure stockée
                    if (medecin.ServicesOfferts?.Any() == true)
                    {
                        foreach (var service in medecin.ServicesOfferts)
                        {
                            await existingConnection.ExecuteAsync(
                                "sp_InsertMedecinService",
                                new { MedecinId = medecin.Id, ServiceId = service.Id },
                                transaction,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    transaction.Commit();
                    return medecin;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            else
            {
                // Sinon, utilisez la méthode existante avec sa propre connexion
                return await ExecuteTransactionAsync(async (conn, transaction) =>
                {
                    // Code existant...
                    // 1. Insérer le médecin avec procédure stockée
                    medecin.Id = await conn.QuerySingleAsync<Guid>(
                        "sp_InsertMedecin",
                        new
                        {
                            medecin.NumeroLicence,
                            medecin.UserId,
                            medecin.Nom,
                            medecin.Prenom,
                            medecin.Specialite,
                            medecin.Telephone,
                            medecin.Email,
                            medecin.AdresseCabinet,
                            medecin.CodePostal,
                            medecin.Ville,
                            medecin.PhotoUrl,
                            medecin.PhotoNom,
                            medecin.PhotoType,
                            medecin.PhotoTaille
                        },
                        transaction,
                        commandType: CommandType.StoredProcedure
                    );

                    // 2. Associer les services médicaux via procédure stockée
                    if (medecin.ServicesOfferts?.Any() == true)
                    {
                        foreach (var service in medecin.ServicesOfferts)
                        {
                            await conn.ExecuteAsync(
                                "sp_InsertMedecinService",
                                new { MedecinId = medecin.Id, ServiceId = service.Id },
                                transaction,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    return medecin;
                });
            }
        }
        public async Task<IEnumerable<Medecin>> GetAllAsync()
        {
            using var conn = _connection.CreateConnection();
            try
            {
                conn.Open();

                // Utiliser la procédure stockée
                var medecins = await conn.QueryAsync<Medecin>(
                    "sp_GetAllMedecins",
                    commandType: CommandType.StoredProcedure
                );

                foreach (var medecin in medecins)
                {
                    // Récupérer les services associés avec procédure stockée
                    var services = await conn.QueryAsync<ServiceMedical>(
                        "sp_GetServicesByMedecinId",
                        new { MedecinId = medecin.Id },
                        commandType: CommandType.StoredProcedure
                    );
                    medecin.ServicesOfferts = services.ToList();
                }

                return medecins;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de tous les médecins");
                throw;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        //UPDATE
        public async Task<Medecin> UpdateAsync(Medecin medecin)
        {
            _logger.LogInformation($"Début de mise à jour complète du médecin avec ID: {medecin.Id}");

            try
            {
                // 1. Conserver la liste des services pour la mise à jour ultérieure
                var serviceOfferts = medecin.ServicesOfferts?.Select(s => s.Id).ToList();

                // 2. Définir ServicesOfferts à null pour éviter les problèmes de clé étrangère
                // lors de la mise à jour des informations du médecin
                medecin.ServicesOfferts = null;

                // 3. Mettre à jour les informations du médecin (sans les services)
                var updatedMedecin = await UpdateMedecinInfoAsync(medecin);

                // 4. Mettre à jour les services séparément
                if (serviceOfferts != null && serviceOfferts.Any())
                {
                    try
                    {
                        await UpdateServicesAsync(updatedMedecin.Id, serviceOfferts);

                        // 5. Récupérer à nouveau le médecin pour avoir les services à jour
                        updatedMedecin = await GetByIdAsync(updatedMedecin.Id);
                    }
                    catch (Exception ex)
                    {
                        // Même si la mise à jour des services échoue, on ne fait pas échouer la mise à jour du médecin
                        _logger.LogWarning(ex, "Erreur lors de la mise à jour des services du médecin {MedecinId}", updatedMedecin.Id);
                    }
                }

                return updatedMedecin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour complète du médecin {MedecinId}", medecin.Id);
                throw;
            }
        }

        // Méthode pour mettre à jour seulement les informations du médecin (sans les services)
        public async Task<Medecin> UpdateMedecinInfoAsync(Medecin medecin)
        {
            _logger.LogInformation($"Début de mise à jour des informations du médecin avec ID: {medecin.Id}");

            await ExecuteTransactionAsync(async (conn, transaction) =>
            {
                // Mettre à jour uniquement les informations du médecin avec procédure stockée
                var rowsAffected = await conn.ExecuteAsync(
                    "sp_UpdateMedecin",
                    new
                    {
                        medecin.Id,
                        medecin.UserId,
                        medecin.NumeroLicence,
                        medecin.Nom,
                        medecin.Prenom,
                        medecin.Specialite,
                        medecin.Telephone,
                        medecin.Email,
                        medecin.AdresseCabinet,
                        medecin.CodePostal,
                        medecin.Ville,
                        UpdatedAt = DateTime.UtcNow,
                        medecin.PhotoUrl,
                        medecin.PhotoNom,
                        medecin.PhotoType,
                        medecin.PhotoTaille
                    },
                    transaction,
                    commandType: CommandType.StoredProcedure
                );

                if (rowsAffected == 0)
                    throw new NotFoundException($"Médecin avec ID {medecin.Id} non trouvé");

                return true;
            });

            _logger.LogInformation($"Mise à jour des informations du médecin terminée avec succès. ID: {medecin.Id}");

            // Récupérer le médecin mis à jour
            var updatedMedecin = await GetByIdAsync(medecin.Id);

            if (updatedMedecin == null)
            {
                throw new NotFoundException($"Médecin avec ID {medecin.Id} non trouvé après mise à jour");
            }

            return updatedMedecin;
        }

        // Méthode dédiée pour gérer les associations de services avec procédures stockées
        public async Task UpdateServicesAsync(Guid medecinId, List<Guid> serviceIds)
        {
            _logger.LogInformation($"Début de mise à jour des services pour le médecin avec ID: {medecinId}");

            try
            {
                await ExecuteTransactionAsync(async (conn, transaction) =>
                {
                    // 1. Supprimer les associations existantes avec procédure stockée
                    await conn.ExecuteAsync(
                        "sp_DeleteMedecinServices",
                        new { MedecinId = medecinId },
                        transaction,
                        commandType: CommandType.StoredProcedure
                    );

                    // 2. Ajouter les nouvelles associations pour les services valides
                    if (serviceIds != null && serviceIds.Any())
                    {
                        // Convertir la liste d'IDs en chaîne CSV pour la procédure stockée
                        var serviceIdsString = string.Join(",", serviceIds);

                        // Obtenir les IDs valides via procédure stockée
                        var validServiceIds = await conn.QueryAsync<Guid>(
                            "sp_GetValidServiceIds",
                            new { ServiceIds = serviceIdsString },
                            transaction,
                            commandType: CommandType.StoredProcedure
                        );

                        _logger.LogInformation($"Services valides trouvés: {validServiceIds.Count()} sur {serviceIds.Count} demandés");

                        // 3. Ajouter les nouvelles associations pour les services valides avec procédure stockée
                        foreach (var serviceId in validServiceIds)
                        {
                            await conn.ExecuteAsync(
                                "sp_InsertMedecinService",
                                new { MedecinId = medecinId, ServiceId = serviceId },
                                transaction,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    return true;
                });

                _logger.LogInformation($"Mise à jour des services terminée avec succès pour le médecin ID: {medecinId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour des services pour le médecin {MedecinId}", medecinId);
                throw;
            }
        }

        // DELETE
        public async Task DeleteAsync(Guid id)
        {
            await ExecuteTransactionAsync(async (conn, transaction) =>
            {
                // 1. Supprimer les associations services avec procédure stockée
                await conn.ExecuteAsync(
                    "sp_DeleteMedecinServices",
                    new { MedecinId = id },
                    transaction,
                    commandType: CommandType.StoredProcedure
                );

                // 2. Supprimer le médecin avec procédure stockée
                var rowsAffected = await conn.ExecuteAsync(
                    "sp_DeleteMedecin",
                    new { Id = id },
                    transaction,
                    commandType: CommandType.StoredProcedure
                );

                if (rowsAffected == 0)
                    throw new NotFoundException($"Médecin avec ID {id} non trouvé");

                return true;
            });
        }

        // Méthodes spécifiques avec procédures stockées
        public async Task<IEnumerable<Medecin>> GetBySpecialiteAsync(string specialite)
        {
            using var conn = _connection.CreateConnection();
            try
            {
                conn.Open();

                // Utiliser la procédure stockée
                var medecins = await conn.QueryAsync<Medecin>(
                    "sp_GetMedecinsBySpecialite",
                    new { Specialite = specialite },
                    commandType: CommandType.StoredProcedure
                );

                foreach (var medecin in medecins)
                {
                    // Récupérer les services associés avec procédure stockée
                    var services = await conn.QueryAsync<ServiceMedical>(
                        "sp_GetServicesByMedecinId",
                        new { MedecinId = medecin.Id },
                        commandType: CommandType.StoredProcedure
                    );
                    medecin.ServicesOfferts = services.ToList();
                }

                return medecins;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des médecins par spécialité {Specialite}", specialite);
                throw;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        public async Task<IEnumerable<Medecin>> GetByServiceAsync(Guid serviceId)
        {
            using var conn = _connection.CreateConnection();
            try
            {
                conn.Open();

                // Utiliser la procédure stockée
                var medecins = await conn.QueryAsync<Medecin>(
                    "sp_GetMedecinsByService",
                    new { ServiceId = serviceId },
                    commandType: CommandType.StoredProcedure
                );

                foreach (var medecin in medecins)
                {
                    // Récupérer les services associés avec procédure stockée
                    var services = await conn.QueryAsync<ServiceMedical>(
                        "sp_GetServicesByMedecinId",
                        new { MedecinId = medecin.Id },
                        commandType: CommandType.StoredProcedure
                    );
                    medecin.ServicesOfferts = services.ToList();
                }

                return medecins;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des médecins par service {ServiceId}", serviceId);
                throw;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        // Méthodes utilitaires avec procédures stockées
        public async Task<bool> ExistsAsync(Guid id)
        {
            using var conn = _connection.CreateConnection();
            try
            {
                conn.Open();

                // Utiliser la procédure stockée
                return await conn.ExecuteScalarAsync<bool>(
                    "sp_CheckMedecinExists",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'existence du médecin {MedecinId}", id);
                throw;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        public async Task<bool> IsLicenceUnique(string numeroLicence, Guid? excludeId = null)
        {
            using var conn = _connection.CreateConnection();
            try
            {
                conn.Open();

                // Utiliser la procédure stockée
                return await conn.ExecuteScalarAsync<bool>(
                    "sp_IsLicenceUnique",
                    new { NumeroLicence = numeroLicence, ExcludeId = excludeId },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'unicité de la licence {NumeroLicence}", numeroLicence);
                throw;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        // Méthodes pour gérer les photos
        public async Task<bool> UpdatePhotoAsync(Guid medecinId, string photoUrl, string photoNom, string photoType, long photoTaille)
        {
            using var conn = _connection.CreateConnection();
            try
            {
                conn.Open();

                // Utiliser la procédure stockée
                var rowsAffected = await conn.ExecuteAsync(
                    "sp_UpdateMedecinPhoto",
                    new
                    {
                        MedecinId = medecinId,
                        PhotoUrl = photoUrl,
                        PhotoNom = photoNom,
                        PhotoType = photoType,
                        PhotoTaille = photoTaille,
                        UpdatedAt = DateTime.UtcNow
                    },
                    commandType: CommandType.StoredProcedure
                );

                if (rowsAffected == 0)
                    throw new NotFoundException($"Médecin avec ID {medecinId} non trouvé");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la photo du médecin {MedecinId}", medecinId);
                throw;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        public async Task<bool> RemovePhotoAsync(Guid medecinId)
        {
            using var conn = _connection.CreateConnection();
            try
            {
                conn.Open();

                // Utiliser la procédure stockée
                var rowsAffected = await conn.ExecuteAsync(
                    "sp_RemoveMedecinPhoto",
                    new
                    {
                        MedecinId = medecinId,
                        UpdatedAt = DateTime.UtcNow
                    },
                    commandType: CommandType.StoredProcedure
                );

                if (rowsAffected == 0)
                    throw new NotFoundException($"Médecin avec ID {medecinId} non trouvé");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la photo du médecin {MedecinId}", medecinId);
                throw;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        public async Task<Medecin> CreateWithLicenceCheckAsync(Medecin medecin)
        {
            return await ExecuteTransactionAsync(async (conn, transaction) =>
            {
                // 1. Vérifier si la licence existe déjà
                var existingMedecin = await conn.QueryFirstOrDefaultAsync<Medecin>(
                    "sp_GetMedecinByLicence",
                    new { NumeroLicence = medecin.NumeroLicence },
                    transaction,
                    commandType: CommandType.StoredProcedure
                );

                if (existingMedecin != null)
                {
                    throw new DuplicateLicenceException($"Le numéro de licence {medecin.NumeroLicence} existe déjà");
                }

                // 2. Insérer le médecin
                medecin.Id = await conn.QuerySingleAsync<Guid>(
                    "sp_InsertMedecin",
                    new
                    {
                        medecin.NumeroLicence,
                        medecin.UserId,
                        medecin.Nom,
                        medecin.Prenom,
                        medecin.Specialite,
                        medecin.Telephone,
                        medecin.Email,
                        medecin.AdresseCabinet,
                        medecin.CodePostal,
                        medecin.Ville,
                        medecin.PhotoUrl,
                        medecin.PhotoNom,
                        medecin.PhotoType,
                        medecin.PhotoTaille
                    },
                    transaction,
                    commandType: CommandType.StoredProcedure
                );

                // 3. Associer les services médicaux
                if (medecin.ServicesOfferts?.Any() == true)
                {
                    foreach (var service in medecin.ServicesOfferts)
                    {
                        await conn.ExecuteAsync(
                            "sp_InsertMedecinService",
                            new { MedecinId = medecin.Id, ServiceId = service.Id },
                            transaction,
                            commandType: CommandType.StoredProcedure
                        );
                    }
                }

                return medecin;
            });
        }
    }
}