using CAPSI.Sante.Application.Data;
using CAPSI.Sante.Application.Repositories.SQLserver.Interfaces;
using CAPSI.Sante.Domain.Models.SQLserver;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Infrastructure.SqlServer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnection _connection;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IDatabaseConnection connection,
            ILogger<UserRepository> logger)
        {
            _connection = connection;
            _logger = logger;
        }
 

        public async Task<User> GetByIdAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection(); 
                return await conn.QueryFirstOrDefaultAsync<User>(
                    "sp_GetUserById",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur {Id}", id);
                throw;
            }
        }


        public async Task<User> GetByEmailAsync(string email)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<User>(
                    "sp_GetUserByEmail",
                    new { Email = email },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur par email {Email}", email);
                throw;
            }
        }

        public async Task<User> GetByRefreshTokenAsync(string refreshToken)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<User>(
                    "sp_GetUserByRefreshToken",
                    new { RefreshToken = refreshToken },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur par refresh token");
                throw;
            }
        }


        public async Task<User> AddAsync(User user)
        {
            try
            {
                using var conn = _connection.CreateConnection();

                if (user.UserId == Guid.Empty)
                {
                    user.UserId = Guid.NewGuid();
                }

                // Assurez-vous que ces paramètres correspondent exactement à ceux de votre procédure stockée
                var parameters = new
                {
                    user.Email,
                    user.PasswordHash,
                    user.UserType,  
                    user.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                return await conn.QueryFirstAsync<User>(
                    "sp_InsertUser",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout de l'utilisateur {Email}: {Message}",
                    user.Email, ex.Message);
                throw;
            }
        }

      
        public async Task UpdateAsync(User user)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                await conn.ExecuteAsync(
                    "sp_UpdateUser",
                    new
                    {
                        user.UserId,  // Utilisez UserId au lieu de Id
                        user.Email,
                        user.PasswordHash,
                        user.UserType,  // Utilisez UserType au lieu de Role
                        user.IsActive,
                        user.LastLogin,
                        user.RefreshToken,
                        user.RefreshTokenExpiryTime
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de l'utilisateur {Id}: {Message}",
                    user.UserId, ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var result = await conn.ExecuteScalarAsync<int>(
                    "sp_DeleteUser",
                    new { UserId = id },
                    commandType: CommandType.StoredProcedure
                );
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'utilisateur {Id}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string email)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.ExecuteScalarAsync<bool>(
                    "sp_CheckEmailExists",
                    new { Email = email },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'existence de l'email {Email}", email);
                throw;
            }
        }
    }
}
