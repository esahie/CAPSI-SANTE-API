using CAPSI.Sante.Application.Data;
using CAPSI.Sante.Application.Repositories.SQLserver.Interfaces;
using CAPSI.Sante.Domain.Exceptions;
using CAPSI.Sante.Domain.Models.SQLserver;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPSI.Sante.Infrastructure.SqlServer.Repositories
{
    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly IDatabaseConnection _connection;
        private readonly ILogger<PrescriptionRepository> _logger;

        public PrescriptionRepository(
            IDatabaseConnection connection,
            ILogger<PrescriptionRepository> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task<Prescription> AddAsync(Prescription prescription)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                conn.Open();
                using var transaction = conn.BeginTransaction();

                try
                {
                    // 1. Insérer la prescription
                    prescription.Id = await conn.QuerySingleAsync<Guid>(
                        "sp_InsertPrescription",
                        new
                        {
                            prescription.DossierId,
                            prescription.MedecinId,
                            prescription.DatePrescription,
                            prescription.DateFin,
                            prescription.Instructions
                        },
                        transaction,
                        commandType: CommandType.StoredProcedure
                    );

                    // 2. Insérer les médicaments associés
                    if (prescription.Medicaments != null && prescription.Medicaments.Count > 0)
                    {
                        foreach (var medicament in prescription.Medicaments)
                        {
                            medicament.PrescriptionId = prescription.Id;
                            medicament.Id = await conn.QuerySingleAsync<Guid>(
                                "sp_InsertMedicamentPrescrit",
                                new
                                {
                                    medicament.PrescriptionId,
                                    medicament.NomMedicament,
                                    medicament.Posologie,
                                    medicament.Duree,
                                    medicament.Instructions
                                },
                                transaction,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    transaction.Commit();
                    return prescription;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'ajout de la prescription");
                throw;
            }
        }

        public async Task<Prescription> GetByIdAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();

                // Récupérer la prescription
                var prescription = await conn.QueryFirstOrDefaultAsync<Prescription>(
                    "SELECT * FROM Prescriptions WHERE PrescriptionId = @Id",
                    new { Id = id }
                );

                if (prescription != null)
                {
                    // Récupérer les médicaments associés
                    var medicaments = await conn.QueryAsync<MedicamentPrescrit>(
                        "SELECT * FROM MedicamentsPrescrits WHERE PrescriptionId = @PrescriptionId",
                        new { PrescriptionId = prescription.Id }
                    );

                    prescription.Medicaments = medicaments.AsList();
                }

                return prescription;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la prescription {PrescriptionId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Prescription>> GetByDossierIdAsync(Guid dossierId)
        {
            try
            {
                using var conn = _connection.CreateConnection();

                // Récupérer toutes les prescriptions du dossier
                var prescriptions = await conn.QueryAsync<Prescription>(
                    "SELECT * FROM Prescriptions WHERE DossierId = @DossierId ORDER BY DatePrescription DESC",
                    new { DossierId = dossierId }
                );

                // Pour chaque prescription, récupérer les médicaments associés
                var prescriptionsList = prescriptions.AsList();
                foreach (var prescription in prescriptionsList)
                {
                    var medicaments = await conn.QueryAsync<MedicamentPrescrit>(
                        "SELECT * FROM MedicamentsPrescrits WHERE PrescriptionId = @PrescriptionId",
                        new { PrescriptionId = prescription.Id }
                    );

                    prescription.Medicaments = medicaments.AsList();
                }

                return prescriptionsList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des prescriptions du dossier {DossierId}", dossierId);
                throw;
            }
        }

        public async Task<IEnumerable<Prescription>> GetByMedecinIdAsync(Guid medecinId)
        {
            try
            {
                using var conn = _connection.CreateConnection();

                // Récupérer toutes les prescriptions du médecin
                var prescriptions = await conn.QueryAsync<Prescription>(
                    "SELECT * FROM Prescriptions WHERE MedecinId = @MedecinId ORDER BY DatePrescription DESC",
                    new { MedecinId = medecinId }
                );

                // Pour chaque prescription, récupérer les médicaments associés
                var prescriptionsList = prescriptions.AsList();
                foreach (var prescription in prescriptionsList)
                {
                    var medicaments = await conn.QueryAsync<MedicamentPrescrit>(
                        "SELECT * FROM MedicamentsPrescrits WHERE PrescriptionId = @PrescriptionId",
                        new { PrescriptionId = prescription.Id }
                    );

                    prescription.Medicaments = medicaments.AsList();
                }

                return prescriptionsList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des prescriptions du médecin {MedecinId}", medecinId);
                throw;
            }
        }

        public async Task<Prescription> UpdateAsync(Prescription prescription)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                conn.Open();
                using var transaction = conn.BeginTransaction();

                try
                {
                    // 1. Mettre à jour la prescription
                    var result = await conn.ExecuteAsync(
                        "UPDATE Prescriptions SET DateFin = @DateFin, Instructions = @Instructions WHERE PrescriptionId = @Id",
                        new
                        {
                            Id = prescription.Id,
                            prescription.DateFin,
                            prescription.Instructions
                        },
                        transaction
                    );

                    if (result == 0)
                        throw new NotFoundException($"Prescription avec ID {prescription.Id} non trouvée");

                    // 2. Supprimer tous les médicaments existants
                    await conn.ExecuteAsync(
                        "DELETE FROM MedicamentsPrescrits WHERE PrescriptionId = @PrescriptionId",
                        new { PrescriptionId = prescription.Id },
                        transaction
                    );

                    // 3. Ajouter les nouveaux médicaments
                    if (prescription.Medicaments != null && prescription.Medicaments.Count > 0)
                    {
                        foreach (var medicament in prescription.Medicaments)
                        {
                            medicament.PrescriptionId = prescription.Id;
                            medicament.Id = await conn.QuerySingleAsync<Guid>(
                                "sp_InsertMedicamentPrescrit",
                                new
                                {
                                    medicament.PrescriptionId,
                                    medicament.NomMedicament,
                                    medicament.Posologie,
                                    medicament.Duree,
                                    medicament.Instructions
                                },
                                transaction,
                                commandType: CommandType.StoredProcedure
                            );
                        }
                    }

                    transaction.Commit();
                    return await GetByIdAsync(prescription.Id);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la prescription {PrescriptionId}", prescription.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                conn.Open();
                using var transaction = conn.BeginTransaction();

                try
                {
                    // 1. Supprimer les médicaments associés
                    await conn.ExecuteAsync(
                        "DELETE FROM MedicamentsPrescrits WHERE PrescriptionId = @Id",
                        new { Id = id },
                        transaction
                    );

                    // 2. Supprimer la prescription
                    var result = await conn.ExecuteAsync(
                        "DELETE FROM Prescriptions WHERE PrescriptionId = @Id",
                        new { Id = id },
                        transaction
                    );

                    if (result == 0)
                        throw new NotFoundException($"Prescription avec ID {id} non trouvée");

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la prescription {PrescriptionId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                using var conn = _connection.CreateConnection();
                return await conn.ExecuteScalarAsync<bool>(
                    "SELECT COUNT(1) FROM Prescriptions WHERE PrescriptionId = @Id",
                    new { Id = id }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'existence de la prescription {PrescriptionId}", id);
                throw;
            }
        }
    }
}
