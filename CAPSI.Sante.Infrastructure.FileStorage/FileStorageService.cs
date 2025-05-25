
using CAPSI.Sante.Application.Storage;
using CAPSI.Sante.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Infrastructure.FileStorage
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _basePath;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
        {
            _basePath = configuration["Storage:DocumentsPath"]
                ?? Path.Combine(Directory.GetCurrentDirectory(), "Documents");
            _logger = logger;

            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
        }

        public async Task<string> SaveFileAsync(IFormFile file, string prefix)
        {
            try
            {
                // Créer un nom de fichier unique
                var fileName = $"{prefix}_{DateTime.UtcNow.Ticks}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(_basePath, fileName);

                // Sauvegarder le fichier
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde du fichier");
                throw new StorageException("Erreur lors de la sauvegarde du fichier", ex);
            }
        }

        public async Task<byte[]> GetFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_basePath, filePath);

            if (!File.Exists(fullPath))
                throw new NotFoundException($"Fichier non trouvé : {filePath}");

            return await File.ReadAllBytesAsync(fullPath);
        }

        public async Task DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_basePath, filePath);

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
