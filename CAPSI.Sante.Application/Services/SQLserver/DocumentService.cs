using CAPSI.Sante.Application.DTOs.Documents;
using CAPSI.Sante.Application.Repositories.SQLserver.Interfaces;
using CAPSI.Sante.Application.Services.SQLserver.Interfaces;
using CAPSI.Sante.Application.Storage;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.SQLserver;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.SQLserver
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentMedicalRepository _documentRepository;
        private readonly IFileStorageService _fileStorage;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(
            IDocumentMedicalRepository documentRepository,
            IFileStorageService fileStorage,
            ILogger<DocumentService> logger)
        {
            _documentRepository = documentRepository;
            _fileStorage = fileStorage;
            _logger = logger;
        }

        public async Task<ApiResponse<DocumentMedical>> UploadDocumentAsync(UploadDocumentDto dto, string utilisateur)
        {
            try
            {
                // Valider le type de fichier
                if (!IsValidFileType(dto.File.ContentType))
                    return new ApiResponse<DocumentMedical>
                    {
                        Success = false,
                        Message = "Type de fichier non autorisé"
                    };

                // Sauvegarder le fichier
                var filePath = await _fileStorage.SaveFileAsync(dto.File, $"dossier_{dto.DossierId}");

                // Créer l'entrée dans la base de données
                var document = new DocumentMedical
                {
                    DossierId = dto.DossierId,
                    Type = dto.Type,
                    Titre = dto.Titre,
                    CheminFichier = filePath,
                    TailleFichier = dto.File.Length,
                    ContentType = dto.File.ContentType,
                    DateUpload = DateTime.UtcNow,
                    UploadParUtilisateur = utilisateur
                };

                document = await _documentRepository.AddAsync(document);

                return new ApiResponse<DocumentMedical>
                {
                    Success = true,
                    Data = document,
                    Message = "Document uploadé avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'upload du document");
                return new ApiResponse<DocumentMedical>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de l'upload du document"
                };
            }
        }

        public async Task<ApiResponse<byte[]>> GetDocumentContentAsync(Guid documentId)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                    return new ApiResponse<byte[]>
                    {
                        Success = false,
                        Message = "Document non trouvé"
                    };

                var content = await _fileStorage.GetFileAsync(document.CheminFichier);
                return new ApiResponse<byte[]>
                {
                    Success = true,
                    Data = content,
                    Message = "Contenu du document récupéré avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du document");
                return new ApiResponse<byte[]>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération du document"
                };
            }
        }

        private bool IsValidFileType(string contentType)
        {
            var allowedTypes = new[]
            {
            "application/pdf",
            "image/jpeg",
            "image/png",
            "image/dicom",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };

            return allowedTypes.Contains(contentType.ToLower());
        }

        public async Task<ApiResponse<DocumentMedical>> GetDocumentByIdAsync(Guid id)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(id);
                if (document == null)
                    return new ApiResponse<DocumentMedical>
                    {
                        Success = false,
                        Message = "Document non trouvé"
                    };

                return new ApiResponse<DocumentMedical>
                {
                    Success = true,
                    Data = document
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du document {DocumentId}", id);
                return new ApiResponse<DocumentMedical>
                {
                    Success = false,
                    Message = "Erreur lors de la récupération du document"
                };
            }
        }

        public async Task<ApiResponse<List<DocumentMedical>>> GetDocumentsByDossierAsync(Guid dossierId, string type = null)
        {
            try
            {
                var documents = type == null
                    ? await _documentRepository.GetByDossierIdAsync(dossierId)
                    : await _documentRepository.GetByTypeAsync(dossierId, type);

                return new ApiResponse<List<DocumentMedical>>
                {
                    Success = true,
                    Data = documents.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des documents du dossier {DossierId}", dossierId);
                return new ApiResponse<List<DocumentMedical>>
                {
                    Success = false,
                    Message = "Erreur lors de la récupération des documents"
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteDocumentAsync(Guid id)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(id);
                if (document == null)
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Document non trouvé"
                    };

                // Supprimer le fichier physique
                await _fileStorage.DeleteFileAsync(document.CheminFichier);

                // Supprimer l'entrée dans la base de données
                await _documentRepository.DeleteAsync(id);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Document supprimé avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du document {DocumentId}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Erreur lors de la suppression du document"
                };
            }
        }

        public async Task<ApiResponse<DocumentMedical>> UpdateDocumentAsync(Guid id, UpdateDocumentDto dto)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(id);
                if (document == null)
                    return new ApiResponse<DocumentMedical>
                    {
                        Success = false,
                        Message = "Document non trouvé"
                    };

                document.Titre = dto.Titre;
                document.Type = dto.Type;
                document.DateModification = DateTime.UtcNow;

                await _documentRepository.UpdateAsync(document);

                return new ApiResponse<DocumentMedical>
                {
                    Success = true,
                    Data = document,
                    Message = "Document mis à jour avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du document {DocumentId}", id);
                return new ApiResponse<DocumentMedical>
                {
                    Success = false,
                    Message = "Erreur lors de la mise à jour du document"
                };
            }
        }

        public async Task<ApiResponse<List<DocumentMedical>>> GetRecentDocumentsAsync(Guid dossierId, int count = 5)
        {
            try
            {
                var documents = await _documentRepository.GetRecentDocumentsAsync(dossierId, count);
                return new ApiResponse<List<DocumentMedical>>
                {
                    Success = true,
                    Data = documents.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des documents récents du dossier {DossierId}", dossierId);
                return new ApiResponse<List<DocumentMedical>>
                {
                    Success = false,
                    Message = "Erreur lors de la récupération des documents récents"
                };
            }
        }
    
}
}
