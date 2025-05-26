using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore; // ← Interface native ASP.NET Core
using System;
using System.Collections.Generic;
using System.IO; // ← Pour Path, File, Directory
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.API.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment; // ← Interface native ASP.NET Core
        private readonly ILogger<FileService> _logger;
        private readonly string[] _allowedImageTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder, string fileName = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("Fichier invalide");

                // Créer le dossier s'il n'existe pas
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", folder);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Générer un nom de fichier unique si non fourni
                if (string.IsNullOrEmpty(fileName))
                {
                    var fileExtension = Path.GetExtension(file.FileName);
                    fileName = $"{Guid.NewGuid()}{fileExtension}";
                }

                var filePath = Path.Combine(uploadPath, fileName);

                // Sauvegarder le fichier
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Retourner l'URL relative
                return $"/uploads/{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde du fichier");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;

                // Convertir l'URL en chemin physique
                var physicalPath = filePath.StartsWith("/uploads/")
                    ? Path.Combine(_environment.WebRootPath, filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))
                    : filePath;

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du fichier {FilePath}", filePath);
                return false;
            }
        }

        public async Task<byte[]> GetFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return null;

                // Convertir l'URL en chemin physique
                var physicalPath = filePath.StartsWith("/uploads/")
                    ? Path.Combine(_environment.WebRootPath, filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))
                    : filePath;

                if (File.Exists(physicalPath))
                {
                    return await File.ReadAllBytesAsync(physicalPath);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la lecture du fichier {FilePath}", filePath);
                return null;
            }
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Vérifier la taille
            if (file.Length > _maxFileSize)
                return false;

            // Vérifier le type MIME
            if (!_allowedImageTypes.Contains(file.ContentType.ToLower()))
                return false;

            // Vérifier l'extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            if (!allowedExtensions.Contains(extension))
                return false;

            return true;
        }

        public string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }

        public async Task<string> SavePatientPhotoAsync(IFormFile photo, Guid patientId)
        {
            try
            {
                // Validation du fichier
                if (!IsValidImageFile(photo))
                    throw new ArgumentException("Fichier image invalide");

                // Supprimer l'ancienne photo s'il y en a une
                await DeleteOldPatientPhotoAsync(patientId);

                // Générer un nom de fichier spécifique au patient
                var fileExtension = Path.GetExtension(photo.FileName);
                var fileName = $"patient_{patientId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{fileExtension}";

                // Sauvegarder dans le dossier patients
                return await SaveFileAsync(photo, "patients", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde de la photo du patient {PatientId}", patientId);
                throw;
            }
        }

        public async Task<bool> DeletePatientPhotoAsync(string photoUrl)
        {
            return await DeleteFileAsync(photoUrl);
        }

        private async Task DeleteOldPatientPhotoAsync(Guid patientId)
        {
            try
            {
                var patientsFolder = Path.Combine(_environment.WebRootPath, "uploads", "patients");
                if (!Directory.Exists(patientsFolder))
                    return;

                // Rechercher les anciennes photos du patient
                var oldPhotos = Directory.GetFiles(patientsFolder, $"patient_{patientId}_*");

                foreach (var oldPhoto in oldPhotos)
                {
                    try
                    {
                        File.Delete(oldPhoto);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Impossible de supprimer l'ancienne photo {PhotoPath}", oldPhoto);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression des anciennes photos du patient {PatientId}", patientId);
            }
        }
    }
}