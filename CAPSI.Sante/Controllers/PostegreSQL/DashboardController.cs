using CAPSI.Sante.Application.Services.SQLserver.Interfaces;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.PostgreSQL;
using Microsoft.AspNetCore.Mvc;

namespace CAPSI.Sante.API.Controllers.PostegreSQL
{
    public class DashboardController : BaseApiController
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        [HttpGet("statistiques-globales")]
        [ProducesResponseType(typeof(ApiResponse<StatistiquesGlobales>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<StatistiquesGlobales>>> GetStatistiquesGlobales(
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var response = await _dashboardService.GetStatistiquesGlobalesAsync(dateDebut, dateFin);
            return HandleResponse(response);
        }

        [HttpGet("tendances-sante")]
        [ProducesResponseType(typeof(ApiResponse<List<TendanceSante>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<TendanceSante>>>> GetTendancesSante(
            [FromQuery] DateTime dateDebut,
            [FromQuery] DateTime dateFin)
        {
            var response = await _dashboardService.GetTendancesSanteAsync(dateDebut, dateFin);
            return HandleResponse(response);
        }
    }
}
