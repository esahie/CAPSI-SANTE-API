using Microsoft.Extensions.Logging;
using Dapper;
using CAPSI.Sante.Domain.Models.Firestore;
using CAPSI.Sante.Infrastructure.PostgreSQL.Data;
using CAPSI.Sante.Domain.Models.PostgreSQL;
using CAPSI.Sante.Application.Repositories.PostegreSQL.Interfaces;

namespace CAPSI.Sante.Infrastructure.PostgreSQL.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly PostgresConnection _postgresConnection;
        private readonly ILogger<AnalyticsRepository> _logger;

        public AnalyticsRepository(PostgresConnection postgresConnection, ILogger<AnalyticsRepository> logger)
        {
            _postgresConnection = postgresConnection;
            _logger = logger;
        }



        public async Task<StatistiquesConsultation> GetByIdAsync(Guid id)
        {
            using var conn = _postgresConnection.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<StatistiquesConsultation>(
                "SELECT * FROM statistiques_consultations WHERE id = @Id",
                new { Id = id }
            );

          
        }

        public async Task<IEnumerable<StatistiquesConsultation>> GetAllAsync()
        {
            using var conn = _postgresConnection.CreateConnection();
            return await conn.QueryAsync<StatistiquesConsultation>(
                "SELECT * FROM statistiques_consultations"
            );
        }

        public async Task EnregistrerStatistiquesConsultationAsync(StatistiquesConsultation stats)
        {
            try
            {
                using var conn = _postgresConnection.CreateConnection();
                await conn.ExecuteAsync(@"
                INSERT INTO statistiques_consultations 
                (medecin_id, date, nombre_consultations, duree_moyenne_consultation, taux_occupation)
                VALUES (@MedecinId, @Date, @NombreConsultations, @DureeMoyenneConsultation, @TauxOccupation)",
                    stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement des statistiques");
                throw;
            }
        }

        public async Task EnregistrerAnnulationAsync(RendezVous rdv, string motif)
        {
            try
            {
                using var conn = _postgresConnection.CreateConnection();
                await conn.ExecuteAsync(@"
                INSERT INTO historique_rendez_vous
                (rendez_vous_id, patient_id, medecin_id, date_heure, statut, motif_annulation)
                VALUES (@Id, @PatientId, @MedecinId, @DateHeure, 'Annulé', @Motif)",
                    new { rdv.Id, rdv.PatientId, rdv.MedecinId, rdv.DateHeure, Motif = motif });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement de l'annulation");
                throw;
            }
        }

        public async Task<StatistiquesGlobales> GetStatistiquesGlobalesAsync(DateTime dateDebut, DateTime dateFin)
        {
            using var conn = _postgresConnection.CreateConnection();
            var stats = await conn.QueryFirstOrDefaultAsync<StatistiquesGlobales>(@"
            SELECT 
                COUNT(*) as TotalConsultations,
                COUNT(DISTINCT medecin_id) as NombreMedecinsActifs,
                AVG(taux_occupation) as TauxOccupationMoyen
            FROM statistiques_consultations
            WHERE date BETWEEN @DateDebut AND @DateFin",
                new { DateDebut = dateDebut, DateFin = dateFin });

            stats.StatistiquesParMois = (await conn.QueryAsync<StatistiquesMensuelles>(@"
            SELECT 
                DATE_TRUNC('month', date) as Mois,
                COUNT(*) as NombreConsultations,
                AVG(taux_occupation) as TauxOccupation
            FROM statistiques_consultations
            WHERE date BETWEEN @DateDebut AND @DateFin
            GROUP BY DATE_TRUNC('month', date)
            ORDER BY Mois",
                new { DateDebut = dateDebut, DateFin = dateFin })).ToList();

            return stats;
        }

        public async Task<List<TendanceSante>> GetTendancesSanteAsync(DateTime dateDebut, DateTime dateFin)
        {
            using var conn = _postgresConnection.CreateConnection();
            var tendances = await conn.QueryAsync<TendanceSante>(@"
            SELECT *
            FROM tendances_sante
            WHERE periode_debut >= @DateDebut AND periode_fin <= @DateFin
            ORDER BY periode_debut",
                new { DateDebut = dateDebut, DateFin = dateFin });

            return tendances.ToList();
        }

        // Implémentations des méthodes AddAsync, UpdateAsync, et DeleteAsync de IBaseRepository
        public async Task<StatistiquesConsultation> AddAsync(StatistiquesConsultation entity)
        {
            await EnregistrerStatistiquesConsultationAsync(entity);
            return entity;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            using var conn = _postgresConnection.CreateConnection();
            return await conn.ExecuteScalarAsync<bool>(
                "SELECT COUNT(1) > 0 FROM statistiques_consultations WHERE id = @Id",
                new { Id = id }
            );
        }

        public async Task<StatistiquesConsultation> UpdateAsync(StatistiquesConsultation entity)
        {
            using var conn = _postgresConnection.CreateConnection();
            await conn.ExecuteAsync(@"
    UPDATE statistiques_consultations 
    SET nombre_consultations = @NombreConsultations, 
        duree_moyenne_consultation = @DureeMoyenneConsultation,
        taux_occupation = @TauxOccupation
    WHERE medecin_id = @MedecinId AND date = @Date",
                entity);

            // Retourner l'entité mise à jour
            return entity;
        }
        public async Task DeleteAsync(Guid id)
        {
            using var conn = _postgresConnection.CreateConnection();
            await conn.ExecuteAsync(
                "DELETE FROM statistiques_consultations WHERE id = @Id",
                new { Id = id });
        }
    }
}
