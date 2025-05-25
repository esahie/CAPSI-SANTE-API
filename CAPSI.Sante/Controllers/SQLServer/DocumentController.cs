using CAPSI.Sante.Application.DTOs.Documents;
using CAPSI.Sante.Application.Services.SQLserver.Interfaces;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.SQLserver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CAPSI.Sante.API.Controllers.SQLServer
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : BaseApiController
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentController> _logger;

        [HttpPost("upload")]
        [ProducesResponseType(typeof(ApiResponse<DocumentMedical>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<DocumentMedical>), StatusCodes.Status400BadRequest)]
        [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB limit
        public async Task<ActionResult<ApiResponse<DocumentMedical>>> UploadDocument(
            [FromForm] UploadDocumentDto dto)
        {
            var response = await _documentService.UploadDocumentAsync(dto, User.Identity.Name);
            if (!response.Success)
                return BadRequest(response);

            return CreatedAtAction(nameof(GetDocument), new { id = response.Data.Id }, response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DocumentMedical>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DocumentMedical>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DocumentMedical>>> GetDocument(Guid id)
        {
            var response = await _documentService.GetDocumentByIdAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("{id}/content")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocumentContent(Guid id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (!document.Success)
                return NotFound(document);

            var content = await _documentService.GetDocumentContentAsync(id);
            if (!content.Success)
                return NotFound(content);

            return File(content.Data, document.Data.ContentType, document.Data.Titre);
        }

        [HttpGet("dossier/{dossierId}")]
        [ProducesResponseType(typeof(ApiResponse<List<DocumentMedical>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<DocumentMedical>>>> GetDocumentsByDossier(
            Guid dossierId,
            [FromQuery] string type = null)
        {
            var response = await _documentService.GetDocumentsByDossierAsync(dossierId, type);
            return Ok(response);
        }
    }
}
