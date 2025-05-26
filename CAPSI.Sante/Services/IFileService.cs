using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.API.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder, string fileName = null);
        Task<bool> DeleteFileAsync(string filePath);
        Task<byte[]> GetFileAsync(string filePath);
        bool IsValidImageFile(IFormFile file);
        string GetContentType(string fileName);
        Task<string> SavePatientPhotoAsync(IFormFile photo, Guid patientId);
        Task<bool> DeletePatientPhotoAsync(string photoUrl);
    }
}
