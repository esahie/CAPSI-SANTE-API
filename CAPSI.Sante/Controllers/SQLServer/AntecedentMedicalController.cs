using CAPSI.Sante.Application.DTOs.AntecedentMedical;
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
    public class AntecedentMedicalController : BaseApiController
    {
        private readonly IAntecedentMedicalService _antecedentService;
        private readonly ILogger<AntecedentMedicalController> _logger;

        public AntecedentMedicalController(
            IAntecedentMedicalService antecedentService,
            ILogger<AntecedentMedicalController> logger)
        {
            _antecedentService = antecedentService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AntecedentMedical>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<AntecedentMedical>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AntecedentMedical>>> CreateAntecedent([FromBody] CreateAntecedentMedicalDto dto)
        {
            var response = await _antecedentService.CreateAntecedentAsync(dto);
            if (!response.Success)
                return BadRequest(response);

            return CreatedAtAction(nameof(GetAntecedent), new { id = response.Data.Id }, response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AntecedentMedical>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AntecedentMedical>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AntecedentMedical>>> GetAntecedent(Guid id)
        {
            var response = await _antecedentService.GetAntecedentByIdAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("dossier/{dossierId}")]
        [ProducesResponseType(typeof(ApiResponse<List<AntecedentMedical>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AntecedentMedical>>>> GetAntecedentsByDossier(
            Guid dossierId,
            [FromQuery] string type = null)
        {
            var response = await _antecedentService.GetAntecedentsByDossierAsync(dossierId, type);
            return Ok(response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AntecedentMedical>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AntecedentMedical>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<AntecedentMedical>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AntecedentMedical>>> UpdateAntecedent(
            Guid id,
            [FromBody] UpdateAntecedentMedicalDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new ApiResponse<AntecedentMedical> { Success = false, Message = "ID incohérent" });

            var response = await _antecedentService.UpdateAntecedentAsync(dto);
            if (!response.Success)
                return response.Message.Contains("non trouvé") ? NotFound(response) : BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAntecedent(Guid id)
        {
            var response = await _antecedentService.DeleteAntecedentAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }
    }
}
