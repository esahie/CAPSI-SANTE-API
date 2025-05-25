using CAPSI.Sante.Application.Repositories.PostegreSQL.Interfaces;
using CAPSI.Sante.Application.Services.SQLserver.Interfaces;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.PostgreSQL;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.PostegreSQL
{
    public class DashboardService : IDashboardService
    {
        private readonly IAnalyticsRepository _analyticsRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IAnalyticsRepository analyticsRepository,
            ILogger<DashboardService> logger)
        {
            _analyticsRepository = analyticsRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<StatistiquesGlobales>> GetStatistiquesGlobalesAsync(DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                var stats = await _analyticsRepository.GetStatistiquesGlobalesAsync(dateDebut, dateFin);
                return new ApiResponse<StatistiquesGlobales>
                {
                    Success = true,
                    Data = stats,
                    Message = "Statistiques globales récupérées avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques globales");
                return new ApiResponse<StatistiquesGlobales>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des statistiques"
                };
            }
        }

        public async Task<ApiResponse<List<TendanceSante>>> GetTendancesSanteAsync(DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                var tendances = await _analyticsRepository.GetTendancesSanteAsync(dateDebut, dateFin);
                return new ApiResponse<List<TendanceSante>>
                {
                    Success = true,
                    Data = tendances,
                    Message = "Tendances de santé récupérées avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des tendances de santé");
                return new ApiResponse<List<TendanceSante>>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des tendances"
                };
            }
        }
    }
}
