using CAPSI.Sante.Application.DTOs.ServiceMedical;
using CAPSI.Sante.Application.Services.SQLserver.Interfaces;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.SQLserver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CAPSI.Sante.API.Controllers.SQLServer
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceMedicalController : BaseApiController
    {
        private readonly IServiceMedicalService _serviceMedicalService;
        private readonly ILogger<ServiceMedicalController> _logger;

        public ServiceMedicalController(IServiceMedicalService serviceMedicalService, ILogger<ServiceMedicalController> logger)
        {
            _serviceMedicalService = serviceMedicalService;
            _logger = logger;
        }

        // GET: api/ServiceMedical
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ServiceMedical>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ServiceMedical>>>> GetAllServices([FromQuery] bool includeInactive = false)
        {
            _logger.LogInformation("Requête entrante GET: api/ServiceMedical - Récupération de tous les services médicaux (includeInactive: {IncludeInactive})", includeInactive);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await _serviceMedicalService.GetAllServicesAsync(includeInactive);
                stopwatch.Stop();

                _logger.LogInformation("GET: api/ServiceMedical - {Count} services récupérés en {ElapsedMilliseconds}ms",
                    response.Data?.Count ?? 0, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erreur lors de la récupération de tous les services médicaux - Temps écoulé: {ElapsedMilliseconds}ms",
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        // GET: api/ServiceMedical/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ServiceMedical>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ServiceMedical>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ServiceMedical>>> GetService(Guid id)
        {
            _logger.LogInformation("Requête entrante GET: api/ServiceMedical/{Id}", id);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await _serviceMedicalService.GetServiceByIdAsync(id);
                stopwatch.Stop();

                if (response.Success)
                {
                    _logger.LogInformation("GET: api/ServiceMedical/{Id} - Service récupéré avec succès en {ElapsedMilliseconds}ms",
                        id, stopwatch.ElapsedMilliseconds);
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("GET: api/ServiceMedical/{Id} - Service non trouvé en {ElapsedMilliseconds}ms",
                        id, stopwatch.ElapsedMilliseconds);
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erreur lors de la récupération du service {Id} - Temps écoulé: {ElapsedMilliseconds}ms",
                    id, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        // GET: api/ServiceMedical/code/{code}
        [HttpGet("code/{code}")]
        [ProducesResponseType(typeof(ApiResponse<ServiceMedical>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ServiceMedical>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ServiceMedical>>> GetServiceByCode(string code)
        {
            _logger.LogInformation("Requête entrante GET: api/ServiceMedical/code/{Code}", code);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await _serviceMedicalService.GetServiceByCodeAsync(code);
                stopwatch.Stop();

                if (response.Success)
                {
                    _logger.LogInformation("GET: api/ServiceMedical/code/{Code} - Service récupéré avec succès en {ElapsedMilliseconds}ms",
                        code, stopwatch.ElapsedMilliseconds);
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("GET: api/ServiceMedical/code/{Code} - Service non trouvé en {ElapsedMilliseconds}ms",
                        code, stopwatch.ElapsedMilliseconds);
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erreur lors de la récupération du service par code {Code} - Temps écoulé: {ElapsedMilliseconds}ms",
                    code, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        // GET: api/ServiceMedical/medecin/{medecinId}
        [HttpGet("medecin/{medecinId}")]
        [ProducesResponseType(typeof(ApiResponse<List<ServiceMedical>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<ServiceMedical>>>> GetServicesByMedecin(Guid medecinId, [FromQuery] bool includeInactive = false)
        {
            _logger.LogInformation("Requête entrante GET: api/ServiceMedical/medecin/{MedecinId} (includeInactive: {IncludeInactive})", medecinId, includeInactive);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await _serviceMedicalService.GetServicesByMedecinAsync(medecinId, includeInactive);
                stopwatch.Stop();

                _logger.LogInformation("GET: api/ServiceMedical/medecin/{MedecinId} - {Count} services récupérés en {ElapsedMilliseconds}ms",
                    medecinId, response.Data?.Count ?? 0, stopwatch.ElapsedMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erreur lors de la récupération des services du médecin {MedecinId} - Temps écoulé: {ElapsedMilliseconds}ms",
                    medecinId, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        // POST: api/ServiceMedical
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ServiceMedical>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<ServiceMedical>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ServiceMedical>>> CreateService([FromBody] CreateServiceMedicalDto dto)
        {
            _logger.LogInformation("Requête entrante POST: api/ServiceMedical - Création d'un nouveau service avec code {Code}", dto.Code);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await _serviceMedicalService.CreateServiceAsync(dto);
                stopwatch.Stop();

                if (response.Success)
                {
                    _logger.LogInformation("POST: api/ServiceMedical - Service créé avec succès (ID: {Id}) en {ElapsedMilliseconds}ms",
                        response.Data.Id, stopwatch.ElapsedMilliseconds);
                    return CreatedAtAction(nameof(GetService), new { id = response.Data.Id }, response);
                }
                else
                {
                    _logger.LogWarning("POST: api/ServiceMedical - Échec de la création du service: {Message} en {ElapsedMilliseconds}ms",
                        response.Message, stopwatch.ElapsedMilliseconds);
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erreur lors de la création d'un nouveau service - Temps écoulé: {ElapsedMilliseconds}ms",
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        // PUT: api/ServiceMedical/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ServiceMedical>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ServiceMedical>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<ServiceMedical>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ServiceMedical>>> UpdateService(Guid id, [FromBody] UpdateServiceMedicalDto dto)
        {
            _logger.LogInformation("Requête entrante PUT: api/ServiceMedical/{Id} - Mise à jour du service", id);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (id != dto.Id)
                {
                    _logger.LogWarning("PUT: api/ServiceMedical/{Id} - L'ID dans l'URL ({UrlId}) ne correspond pas à l'ID dans les données ({DtoId})",
                        id, dto.Id);

                    return BadRequest(new ApiResponse<ServiceMedical>
                    {
                        Success = false,
                        Message = "L'ID dans l'URL ne correspond pas à l'ID dans les données"
                    });
                }

                var response = await _serviceMedicalService.UpdateServiceAsync(dto);
                stopwatch.Stop();

                if (response.Success)
                {
                    _logger.LogInformation("PUT: api/ServiceMedical/{Id} - Service mis à jour avec succès en {ElapsedMilliseconds}ms",
                        id, stopwatch.ElapsedMilliseconds);
                    return Ok(response);
                }
                else
                {
                    if (response.Message == "Service non trouvé")
                    {
                        _logger.LogWarning("PUT: api/ServiceMedical/{Id} - Service non trouvé en {ElapsedMilliseconds}ms",
                            id, stopwatch.ElapsedMilliseconds);
                        return NotFound(response);
                    }
                    else
                    {
                        _logger.LogWarning("PUT: api/ServiceMedical/{Id} - Échec de la mise à jour: {Message} en {ElapsedMilliseconds}ms",
                            id, response.Message, stopwatch.ElapsedMilliseconds);
                        return BadRequest(response);
                    }
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erreur lors de la mise à jour du service {Id} - Temps écoulé: {ElapsedMilliseconds}ms",
                    id, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        // PUT: api/ServiceMedical/{id}/activate
        [HttpPut("{id}/activate")]
        [ProducesResponseType(typeof(ApiResponse<ServiceMedical>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ServiceMedical>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ServiceMedical>>> ActivateService(Guid id)
        {
            _logger.LogInformation("Requête entrante PUT: api/ServiceMedical/{Id}/activate - Activation du service", id);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await _serviceMedicalService.ActivateServiceAsync(id);
                stopwatch.Stop();

                if (response.Success)
                {
                    _logger.LogInformation("PUT: api/ServiceMedical/{Id}/activate - Service activé avec succès en {ElapsedMilliseconds}ms",
                        id, stopwatch.ElapsedMilliseconds);
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("PUT: api/ServiceMedical/{Id}/activate - Service non trouvé en {ElapsedMilliseconds}ms",
                        id, stopwatch.ElapsedMilliseconds);
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erreur lors de l'activation du service {Id} - Temps écoulé: {ElapsedMilliseconds}ms",
                    id, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        // PUT: api/ServiceMedical/{id}/deactivate
        [HttpPut("{id}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<ServiceMedical>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ServiceMedical>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ServiceMedical>>> DeactivateService(Guid id)
        {
            _logger.LogInformation("Requête entrante PUT: api/ServiceMedical/{Id}/deactivate - Désactivation du service", id);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await _serviceMedicalService.DeactivateServiceAsync(id);
                stopwatch.Stop();

                if (response.Success)
                {
                    _logger.LogInformation("PUT: api/ServiceMedical/{Id}/deactivate - Service désactivé avec succès en {ElapsedMilliseconds}ms",
                        id, stopwatch.ElapsedMilliseconds);
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("PUT: api/ServiceMedical/{Id}/deactivate - Service non trouvé en {ElapsedMilliseconds}ms",
                        id, stopwatch.ElapsedMilliseconds);
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erreur lors de la désactivation du service {Id} - Temps écoulé: {ElapsedMilliseconds}ms",
                    id, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        // DELETE: api/ServiceMedical/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteService(Guid id)
        {
            _logger.LogInformation("Requête entrante DELETE: api/ServiceMedical/{Id} - Suppression définitive du service", id);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await _serviceMedicalService.DeleteServiceAsync(id);
                stopwatch.Stop();

                if (response.Success)
                {
                    _logger.LogInformation("DELETE: api/ServiceMedical/{Id} - Service supprimé avec succès en {ElapsedMilliseconds}ms",
                        id, stopwatch.ElapsedMilliseconds);
                    return Ok(response);
                }
                else
                {
                    if (response.Message == "Service non trouvé")
                    {
                        _logger.LogWarning("DELETE: api/ServiceMedical/{Id} - Service non trouvé en {ElapsedMilliseconds}ms",
                            id, stopwatch.ElapsedMilliseconds);
                        return NotFound(response);
                    }
                    else if (response.Message.Contains("utilisé dans des rendez-vous"))
                    {
                        _logger.LogWarning("DELETE: api/ServiceMedical/{Id} - Service utilisé dans des rendez-vous: {Message} en {ElapsedMilliseconds}ms",
                            id, response.Message, stopwatch.ElapsedMilliseconds);
                        return BadRequest(response);
                    }
                    else
                    {
                        _logger.LogWarning("DELETE: api/ServiceMedical/{Id} - Échec de la suppression: {Message} en {ElapsedMilliseconds}ms",
                            id, response.Message, stopwatch.ElapsedMilliseconds);
                        return BadRequest(response);
                    }
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Erreur lors de la suppression du service {Id} - Temps écoulé: {ElapsedMilliseconds}ms",
                    id, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}