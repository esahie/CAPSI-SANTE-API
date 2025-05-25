using CAPSI.Sante.Application.DTOs.Prescription;
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
    public class PrescriptionController : BaseApiController
    {
        private readonly IPrescriptionService _prescriptionService;
        private readonly ILogger<PrescriptionController> _logger;

        public PrescriptionController(
            IPrescriptionService prescriptionService,
            ILogger<PrescriptionController> logger)
        {
            _prescriptionService = prescriptionService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Prescription>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<Prescription>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<Prescription>>> CreatePrescription([FromBody] CreatePrescriptionDto dto)
        {
            try
            {
                var response = await _prescriptionService.CreatePrescriptionAsync(dto);
                if (!response.Success)
                    return BadRequest(response);

                return CreatedAtAction(nameof(GetPrescription), new { id = response.Data.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création d'une prescription");
                return BadRequest(new ApiResponse<Prescription>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la création de la prescription"
                });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Prescription>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Prescription>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Prescription>>> GetPrescription(Guid id)
        {
            try
            {
                var response = await _prescriptionService.GetPrescriptionByIdAsync(id);
                if (!response.Success)
                    return NotFound(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la prescription {PrescriptionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<Prescription>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération de la prescription"
                });
            }
        }

        [HttpGet("dossier/{dossierId}")]
        [ProducesResponseType(typeof(ApiResponse<List<Prescription>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<Prescription>>>> GetPrescriptionsByDossier(Guid dossierId)
        {
            try
            {
                var response = await _prescriptionService.GetPrescriptionsByDossierAsync(dossierId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des prescriptions du dossier {DossierId}", dossierId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<List<Prescription>>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des prescriptions"
                });
            }
        }

        [HttpGet("medecin/{medecinId}")]
        [ProducesResponseType(typeof(ApiResponse<List<Prescription>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<Prescription>>>> GetPrescriptionsByMedecin(Guid medecinId)
        {
            try
            {
                var response = await _prescriptionService.GetPrescriptionsByMedecinAsync(medecinId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des prescriptions du médecin {MedecinId}", medecinId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<List<Prescription>>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des prescriptions"
                });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Prescription>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Prescription>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Prescription>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Prescription>>> UpdatePrescription(
            Guid id,
            [FromBody] UpdatePrescriptionDto dto)
        {
            try
            {
                if (id != dto.Id)
                    return BadRequest(new ApiResponse<Prescription> { Success = false, Message = "ID incohérent" });

                var response = await _prescriptionService.UpdatePrescriptionAsync(dto);
                if (!response.Success)
                    return response.Message.Contains("non trouvée") ? NotFound(response) : BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la prescription {PrescriptionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<Prescription>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la mise à jour de la prescription"
                });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePrescription(Guid id)
        {
            try
            {
                var response = await _prescriptionService.DeletePrescriptionAsync(id);
                if (!response.Success)
                    return NotFound(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la prescription {PrescriptionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la suppression de la prescription"
                });
            }
        }
    }
}
