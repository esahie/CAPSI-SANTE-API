using CAPSI.Sante.Application.DTOs.DossierMedical;
using CAPSI.Sante.Application.Services.SQLserver.Interfaces;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.SQLserver;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CAPSI.Sante.API.Controllers.SQLServer
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DossierMedicalController : BaseApiController
    {
        private readonly IDossierMedicalService _dossierMedicalService;
        private readonly ILogger<DossierMedicalController> _logger;

        public DossierMedicalController(
            IDossierMedicalService dossierMedicalService,
            ILogger<DossierMedicalController> logger)
        {
            _dossierMedicalService = dossierMedicalService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DossierMedical>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<DossierMedical>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<DossierMedical>>> CreateDossier([FromBody] CreateDossierMedicalDto dto)
        {
            var response = await _dossierMedicalService.CreateDossierAsync(dto);
            if (!response.Success)
                return BadRequest(response);

            return CreatedAtAction(nameof(GetDossier), new { id = response.Data.Id }, response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DossierMedical>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DossierMedical>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DossierMedical>>> GetDossier(Guid id)
        {
            var response = await _dossierMedicalService.GetDossierByIdAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("patient/{patientId}")]
        [ProducesResponseType(typeof(ApiResponse<DossierMedical>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DossierMedical>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DossierMedical>>> GetDossierByPatient(Guid patientId)
        {
            var response = await _dossierMedicalService.GetDossierByPatientIdAsync(patientId);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<DossierMedical>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<DossierMedical>>>> GetAllDossiers()
        {
            var response = await _dossierMedicalService.GetAllDossiersAsync();
            return Ok(response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DossierMedical>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DossierMedical>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<DossierMedical>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DossierMedical>>> UpdateDossier(
            Guid id,
            [FromBody] UpdateDossierMedicalDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new ApiResponse<DossierMedical> { Success = false, Message = "ID incohérent" });

            var response = await _dossierMedicalService.UpdateDossierAsync(dto);
            if (!response.Success)
                return response.Message.Contains("non trouvé") ? NotFound(response) : BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteDossier(Guid id)
        {
            var response = await _dossierMedicalService.DeleteDossierAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }
    }
}
