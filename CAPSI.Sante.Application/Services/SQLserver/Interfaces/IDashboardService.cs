using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.PostgreSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Services.SQLserver.Interfaces
{
    public interface IDashboardService
    {
        Task<ApiResponse<StatistiquesGlobales>> GetStatistiquesGlobalesAsync(DateTime dateDebut, DateTime dateFin);
        Task<ApiResponse<List<TendanceSante>>> GetTendancesSanteAsync(DateTime dateDebut, DateTime dateFin);
    }
}
