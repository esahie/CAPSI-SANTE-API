using CAPSI.Sante.Domain.Models.Firestore;
using CAPSI.Sante.Domain.Models.PostgreSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Application.Repositories.PostegreSQL.Interfaces
{
    public interface IAnalyticsRepository : IBaseRepository<StatistiquesConsultation>
    {
        Task EnregistrerStatistiquesConsultationAsync(StatistiquesConsultation stats);
        Task EnregistrerAnnulationAsync(RendezVous rdv, string motif);
        Task<StatistiquesGlobales> GetStatistiquesGlobalesAsync(DateTime dateDebut, DateTime dateFin);
        Task<List<TendanceSante>> GetTendancesSanteAsync(DateTime dateDebut, DateTime dateFin);
    }
}
