using CAPSI.Sante.Application.DTOs.Documents;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.SQLserver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.SQLserver.Interfaces
{
    public interface IDocumentService
    {
        Task<ApiResponse<DocumentMedical>> UploadDocumentAsync(UploadDocumentDto dto, string utilisateur);
        Task<ApiResponse<DocumentMedical>> GetDocumentByIdAsync(Guid id);
        Task<ApiResponse<byte[]>> GetDocumentContentAsync(Guid id);
        Task<ApiResponse<List<DocumentMedical>>> GetDocumentsByDossierAsync(Guid dossierId, string type = null);
        Task<ApiResponse<bool>> DeleteDocumentAsync(Guid id);
        Task<ApiResponse<DocumentMedical>> UpdateDocumentAsync(Guid id, UpdateDocumentDto dto);
        Task<ApiResponse<List<DocumentMedical>>> GetRecentDocumentsAsync(Guid dossierId, int count = 5);
    }
}
