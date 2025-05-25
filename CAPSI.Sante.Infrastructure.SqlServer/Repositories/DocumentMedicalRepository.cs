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
    public class DocumentMedicalRepository : IDocumentMedicalRepository
    {
        private readonly IDatabaseConnection _connection;
        private readonly ILogger<DocumentMedicalRepository> _logger;


        public DocumentMedicalRepository(IDatabaseConnection connection,
        ILogger<DocumentMedicalRepository> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task<DocumentMedical> AddAsync(DocumentMedical document)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                document.Id = await conn.QuerySingleAsync<Guid>(
                    "sp_InsertDocument",
                    new
                    {
                        document.DossierId,
                        document.Type,
                        document.Titre,
                        document.CheminFichier,
                        document.TailleFichier,
                        document.ContentType,
                        document.UploadParUtilisateur
                    },
                    commandType: CommandType.StoredProcedure
                );

                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout du document");
                throw;
            }
        }

        
        // READ
        public async Task<DocumentMedical> GetByIdAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<DocumentMedical>(
                    "SELECT * FROM DocumentsMedicaux WHERE Id = @Id",
                    new { Id = id }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du document {DocumentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<DocumentMedical>> GetAllAsync()
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryAsync<DocumentMedical>(
                    "SELECT * FROM DocumentsMedicaux ORDER BY DateUpload DESC"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de tous les documents");
                throw;
            }
        }

        public async Task<IEnumerable<DocumentMedical>> GetByDossierIdAsync(Guid dossierId)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryAsync<DocumentMedical>(
                    @"SELECT * FROM DocumentsMedicaux 
                WHERE DossierId = @DossierId 
                ORDER BY DateUpload DESC",
                    new { DossierId = dossierId }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des documents du dossier {DossierId}", dossierId);
                throw;
            }
        }

        public async Task<IEnumerable<DocumentMedical>> GetByPatientIdAsync(Guid patientId)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryAsync<DocumentMedical>(
                    @"SELECT dm.* 
                FROM DocumentsMedicaux dm
                INNER JOIN DossiersMedicaux dos ON dm.DossierId = dos.Id
                WHERE dos.PatientId = @PatientId
                ORDER BY dm.DateUpload DESC",
                    new { PatientId = patientId }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des documents du patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<DocumentMedical>> GetByTypeAsync(Guid dossierId, string type)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryAsync<DocumentMedical>(
                    @"SELECT * FROM DocumentsMedicaux 
                WHERE DossierId = @DossierId 
                AND Type = @Type
                ORDER BY DateUpload DESC",
                    new { DossierId = dossierId, Type = type }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des documents de type {Type} du dossier {DossierId}", type, dossierId);
                throw;
            }
        }

        // UPDATE

        public async Task<DocumentMedical> UpdateAsync(DocumentMedical document)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var result = await conn.ExecuteAsync(
                    @"UPDATE DocumentsMedicaux 
            SET Titre = @Titre,
                Type = @Type,
                DateModification = @DateModification
            WHERE Id = @Id",
                    new
                    {
                        document.Id,
                        document.Titre,
                        document.Type,
                        DateModification = DateTime.UtcNow
                    }
                );

                if (result == 0)
                    throw new NotFoundException($"Document avec ID {document.Id} non trouvé");

                // Retourner le document mis à jour
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du document {DocumentId}", document.Id);
                throw;
            }
        }

        

        // DELETE
        public async Task DeleteAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                var result = await conn.ExecuteAsync(
                    "DELETE FROM DocumentsMedicaux WHERE Id = @Id",
                    new { Id = id }
                );

                if (result == 0)
                    throw new NotFoundException($"Document avec ID {id} non trouvé");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du document {DocumentId}", id);
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
                    "SELECT COUNT(1) FROM DocumentsMedicaux WHERE Id = @Id",
                    new { Id = id }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'existence du document {DocumentId}", id);
                throw;
            }
        }

        public async Task<int> GetDocumentCountByDossierAsync(Guid dossierId)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM DocumentsMedicaux WHERE DossierId = @DossierId",
                    new { DossierId = dossierId }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du comptage des documents du dossier {DossierId}", dossierId);
                throw;
            }
        }

        public async Task<IEnumerable<DocumentMedical>> GetRecentDocumentsAsync(Guid dossierId, int count = 5)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.QueryAsync<DocumentMedical>(
                    @"SELECT TOP(@Count) * 
                FROM DocumentsMedicaux 
                WHERE DossierId = @DossierId 
                ORDER BY DateUpload DESC",
                    new { DossierId = dossierId, Count = count }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des documents récents du dossier {DossierId}", dossierId);
                throw;
            }
        }

        public async Task<IEnumerable<DocumentMedical>> GetAllByPatientAsync(Guid patientId, int page = 1, int pageSize = 10)
        {
            try
            {
                using var conn = _connection.CreateConnection();

                var offset = (page - 1) * pageSize;

                return await conn.QueryAsync<DocumentMedical>(
                    @"SELECT dm.* 
            FROM DocumentsMedicaux dm
            INNER JOIN DossiersMedicaux dos ON dm.DossierId = dos.DossierId
            WHERE dos.PatientId = @PatientId
            ORDER BY dm.DateUpload DESC
            OFFSET @Offset ROWS 
            FETCH NEXT @PageSize ROWS ONLY",
                    new
                    {
                        PatientId = patientId,
                        Offset = offset,
                        PageSize = pageSize
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des documents du patient {PatientId}", patientId);
                throw;
            }
        }

        // Méthode pour compter le nombre total de documents d'un patient
        public async Task<int> CountDocumentsByPatientAsync(Guid patientId)
        {
            try
            {
                using var conn = _connection.CreateConnection();

                return await conn.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(*)
            FROM DocumentsMedicaux dm
            INNER JOIN DossiersMedicaux dos ON dm.DossierId = dos.DossierId
            WHERE dos.PatientId = @PatientId",
                    new { PatientId = patientId }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du comptage des documents du patient {PatientId}", patientId);
                throw;
            }
        }

    }
}
