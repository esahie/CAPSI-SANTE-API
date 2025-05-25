using CAPSI.Sante.Application.DTOs.Medecin;
using CAPSI.Sante.Application.Services.SQLserver.Interfaces;
using CAPSI.Sante.Common;
using CAPSI.Sante.Core.DTOs;
using CAPSI.Sante.Domain.Models.Firestore;
using CAPSI.Sante.Domain.Models.PostgreSQL;
using CAPSI.Sante.Domain.Models.SQLserver;
using Microsoft.AspNetCore.Mvc;

namespace CAPSI.Sante.API.Controllers.SQLServer
{
    public class MedecinController : BaseApiController
    {
        private readonly IMedecinService _medecinService;
        private readonly ILogger<MedecinController> _logger;
        public MedecinController(IMedecinService medecinService, ILogger<MedecinController> logger)
        {
            _medecinService = medecinService;
            _logger = logger;
        }

        [HttpGet("{medecinId}/analytics")]
        [ProducesResponseType(typeof(ApiResponse<MedecinAnalytics>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<MedecinAnalytics>>> GetAnalytics(
            Guid medecinId,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var response = await _medecinService.GetAnalyticsAsync(medecinId, dateDebut, dateFin);
            return HandleResponse(response);
        }

        [HttpPost("{medecinId}/disponibilites")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<bool>>> SetDisponibilites(
            Guid medecinId,
            [FromBody] List<DisponibiliteDto> disponibilites)
        {
            var response = await _medecinService.SetDisponibilitesAsync(medecinId, disponibilites);
            return HandleResponse(response);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Medecin>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<Medecin>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<Medecin>>> CreateMedecin([FromBody] CreateMedecinDto dto)
        {
            var response = await _medecinService.CreateMedecinAsync(dto);
            if (!response.Success)
                return BadRequest(response);

            return CreatedAtAction(nameof(GetMedecin), new { id = response.Data.Id }, response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Medecin>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Medecin>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Medecin>>> GetMedecin(Guid id)
        {
            var response = await _medecinService.GetMedecinByIdAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<Medecin>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<Medecin>>>> GetMedecins(
            [FromQuery] string specialite = null,
            [FromQuery] Guid? serviceId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var response = await _medecinService.GetMedecinsAsync(specialite, serviceId, page, pageSize);
            return Ok(response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Medecin>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Medecin>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Medecin>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Medecin>>> UpdateMedecin(
            Guid id,
            [FromBody] UpdateMedecinDto dto)
        {
            if (id != dto.Id)
            { 
            _logger.LogWarning($"Tentative de mise à jour avec ID incohérent: Route={id}, Body={dto.Id}");

            return BadRequest(new ApiResponse<Medecin> { Success = false, Message = "ID incohérent" });
            }
            // Assurez-vous que l'ID dans le DTO est valide et non un GUID vide
            if (dto.Id == Guid.Empty)
            {
                _logger.LogWarning("Tentative de mise à jour avec un ID vide");
                return BadRequest(new ApiResponse<Medecin> { Success = false, Message = "L'ID du médecin ne peut pas être vide" });
            }

            // Appel au service
            var response = await _medecinService.UpdateMedecinAsync(dto);

            // Gestion des différents cas de réponse
            if (!response.Success)
            {
                if (response.Message.Contains("non trouvé"))
                    return NotFound(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteMedecin(Guid id)
        {
            var response = await _medecinService.DeleteMedecinAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("{id}/disponibilites")]
        [ProducesResponseType(typeof(ApiResponse<List<CreneauDisponible>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<CreneauDisponible>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<CreneauDisponible>>>> GetDisponibilites(
            Guid id,
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var response = await _medecinService.GetDisponibilitesAsync(id, dateDebut, dateFin);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost("{id}/photo")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<string>>> UploadPhoto(
    Guid id,
    IFormFile photo)
        {
            if (photo == null || photo.Length == 0)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Aucun fichier fourni" });
            }

            var response = await _medecinService.UploadPhotoAsync(id, photo);

            if (!response.Success)
            {
                if (response.Message.Contains("non trouvé"))
                    return NotFound(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpDelete("{id}/photo")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePhoto(Guid id)
        {
            var response = await _medecinService.DeletePhotoAsync(id);

            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("{id}/photo")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPhoto(Guid id)
        {
            try
            {
                // Utiliser le service pour récupérer les informations de la photo
                var response = await _medecinService.GetPhotoInfoAsync(id);

                if (!response.Success)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = response.Message
                    });
                }

                // Lire le fichier et le retourner
                var fileInfo = response.Data;
                var fileBytes = System.IO.File.ReadAllBytes(fileInfo.FilePath);

                return File(fileBytes, fileInfo.ContentType ?? "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la photo du médecin {MedecinId}", id);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération de la photo"
                });
            }
        }
    }
}
