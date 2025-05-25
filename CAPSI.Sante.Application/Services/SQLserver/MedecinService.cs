using CAPSI.Sante.Application.DTOs.Medecin;
using CAPSI.Sante.Application.Repositories.PostegreSQL.Interfaces;
using CAPSI.Sante.Application.Repositories.SQLserver.Interfaces;
using CAPSI.Sante.Application.Services.Firebase.Interfaces;
using CAPSI.Sante.Application.Services.SQLserver.Interfaces;
using CAPSI.Sante.Common;
using CAPSI.Sante.Core.DTOs;
using CAPSI.Sante.Domain.Exceptions;
using CAPSI.Sante.Domain.Models.Firestore;
using CAPSI.Sante.Domain.Models.PostgreSQL;
using CAPSI.Sante.Domain.Models.SQLserver;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CAPSI.Sante.Application.Data;

namespace CAPSI.Sante.Application.Services.SQLserver
{
    public class MedecinService : IMedecinService
    {
        private readonly IAnalyticsRepository _analyticsRepository;
        private readonly IFirestoreRendezVousService _firestoreRendezVousService;
        private readonly ILogger<MedecinService> _logger;
        private readonly IMedecinRepository _medecinRepository;
        private readonly IServiceMedicalRepository _serviceMedicalRepository;
        private readonly FirestoreDb _firestoreDb; 

        public MedecinService(
            IAnalyticsRepository analyticsRepository,
            IFirestoreRendezVousService firestoreRendezVousService,
            ILogger<MedecinService> logger,
            IMedecinRepository medecinRepository,
            FirestoreDb firestoreDb,
            IServiceMedicalRepository serviceMedicalRepository )  // Ajouté
        {
            _analyticsRepository = analyticsRepository;
            _firestoreRendezVousService = firestoreRendezVousService;
            _logger = logger;
            _medecinRepository = medecinRepository;
            _serviceMedicalRepository = serviceMedicalRepository;
            _firestoreDb = firestoreDb; 
        }

        public async Task<ApiResponse<Medecin>> CreateMedecinAsync(CreateMedecinDto dto)
        {
            try
            {
                // Vérifier si le numéro de licence existe déjà en utilisant directement le repository
                var existingMedecin = await _medecinRepository.GetByLicenceAsync(dto.NumeroLicence);
                if (existingMedecin != null)
                {
                    return new ApiResponse<Medecin>
                    {
                        Success = false,
                        Message = "Ce numéro de licence existe déjà"
                    };
                }

                // Créer l'entité médecin
                var medecin = new Medecin
                {
                    NumeroLicence = dto.NumeroLicence,
                    UserId = dto.UserId,
                    Nom = dto.Nom,
                    Prenom = dto.Prenom,
                    Specialite = dto.Specialite,
                    Telephone = dto.Telephone,
                    Email = dto.Email,
                    AdresseCabinet = dto.AdresseCabinet,
                    CodePostal = dto.CodePostal,
                    Ville = dto.Ville,
                    PhotoUrl = dto.PhotoUrl,
                    PhotoNom = dto.PhotoNom,
                    PhotoType = dto.PhotoType,
                    PhotoTaille = dto.PhotoTaille,
                    CreatedAt = DateTime.UtcNow
                };

                // Ajouter les services associés si fournis
                if (dto.ServicesOfferts != null && dto.ServicesOfferts.Any())
                {
                    medecin.ServicesOfferts = dto.ServicesOfferts
                        .Select(id => new ServiceMedical { Id = id })
                        .ToList();
                }

                try
                {
                    medecin = await _medecinRepository.CreateWithLicenceCheckAsync(medecin);

                    return new ApiResponse<Medecin>
                    {
                        Success = true,
                        Data = medecin,
                        Message = "Médecin créé avec succès"
                    };
                }
                catch (DuplicateLicenceException ex)
                {
                    return new ApiResponse<Medecin>
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du médecin");
                return new ApiResponse<Medecin>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la création du médecin"
                };
            }
        }
        public async Task<ApiResponse<Medecin>> GetMedecinByIdAsync(Guid id)
        {
            try
            {
                var medecin = await _medecinRepository.GetByIdAsync(id);
                if (medecin == null)
                {
                    return new ApiResponse<Medecin>
                    {
                        Success = false,
                        Message = "Médecin non trouvé"
                    };
                }

                return new ApiResponse<Medecin>
                {
                    Success = true,
                    Data = medecin
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du médecin {MedecinId}", id);
                return new ApiResponse<Medecin>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération du médecin"
                };
            }
        }

        public async Task<ApiResponse<List<Medecin>>> GetMedecinsAsync(
            string specialite = null,
            Guid? serviceId = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                IEnumerable<Medecin> medecins;

                if (!string.IsNullOrEmpty(specialite))
                {
                    medecins = await _medecinRepository.GetBySpecialiteAsync(specialite);
                }
                else if (serviceId.HasValue)
                {
                    medecins = await _medecinRepository.GetByServiceAsync(serviceId.Value);
                }
                else
                {
                    medecins = await _medecinRepository.GetAllAsync();
                }

                var paginatedMedecins = medecins
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new ApiResponse<List<Medecin>>
                {
                    Success = true,
                    Data = paginatedMedecins
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des médecins");
                return new ApiResponse<List<Medecin>>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des médecins"
                };
            }
        }

        public async Task<ApiResponse<List<Medecin>>> GetBySpecialiteAsync(string specialite)
        {
            try
            {
                var medecins = await _medecinRepository.GetBySpecialiteAsync(specialite);
                return new ApiResponse<List<Medecin>>
                {
                    Success = true,
                    Data = medecins.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des médecins par spécialité");
                return new ApiResponse<List<Medecin>>
                {
                    Success = false,
                    Message = "Une erreur est survenue"
                };
            }
        }




        public async Task<ApiResponse<Medecin>> UpdateMedecinAsync(UpdateMedecinDto dto)
        {

            try
            {
                _logger.LogInformation($"Début de la mise à jour du médecin avec ID: {dto.Id}");

                // Récupérer le médecin existant
                var existingMedecin = await _medecinRepository.GetByIdAsync(dto.Id);
                if (existingMedecin == null)
                {
                    _logger.LogWarning($"Médecin avec ID {dto.Id} non trouvé");
                    return new ApiResponse<Medecin>
                    {
                        Success = false,
                        Message = "Médecin non trouvé"
                    };
                }

                _logger.LogInformation($"Médecin trouvé, ID avant mise à jour: {existingMedecin.Id}");

                // Mettre à jour les propriétés
                existingMedecin.NumeroLicence = dto.NumeroLicence;
                existingMedecin.UserId = dto.UserId;
                existingMedecin.Nom = dto.Nom;
                existingMedecin.Prenom = dto.Prenom;
                existingMedecin.Specialite = dto.Specialite;
                existingMedecin.Telephone = dto.Telephone;
                existingMedecin.Email = dto.Email;
                existingMedecin.AdresseCabinet = dto.AdresseCabinet;
                existingMedecin.CodePostal = dto.CodePostal;
                existingMedecin.Ville = dto.Ville;
                existingMedecin.UpdatedAt = DateTime.UtcNow;

                // IMPORTANT: Vérifier que l'ID n'a pas été modifié
                if (existingMedecin.Id != dto.Id)
                {
                    _logger.LogWarning($"L'ID a été modifié pendant la mise à jour! Restauration de l'ID original {dto.Id}");
                    existingMedecin.Id = dto.Id;
                }

                // Convertir les IDs en objets ServiceMedical
                // Cette partie est traitée séparément dans la nouvelle implémentation
                if (dto.ServicesOfferts != null)
                {
                    existingMedecin.ServicesOfferts = dto.ServicesOfferts
                        .Select(id => new ServiceMedical { Id = id })
                        .ToList();
                }

                _logger.LogInformation($"ID après mise à jour des propriétés: {existingMedecin.Id}");

                // Mettre à jour en base de données avec la méthode UpdateAsync modifiée
                // qui gère séparément les informations du médecin et les services
                var updatedMedecin = await _medecinRepository.UpdateAsync(existingMedecin);

                _logger.LogInformation($"Mise à jour réussie, ID du médecin retourné: {updatedMedecin?.Id}");

                return new ApiResponse<Medecin>
                {
                    Success = true,
                    Data = updatedMedecin,
                    Message = "Médecin mis à jour avec succès"
                };
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Médecin non trouvé {MedecinId}", dto.Id);
                return new ApiResponse<Medecin>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du médecin {MedecinId}", dto.Id);
                return new ApiResponse<Medecin>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la mise à jour du médecin"
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteMedecinAsync(Guid id)
        {
            try
            {
                var medecin = await _medecinRepository.GetByIdAsync(id);
                if (medecin == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Médecin non trouvé"
                    };
                }

                await _medecinRepository.DeleteAsync(id);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Médecin supprimé avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du médecin {MedecinId}", id);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la suppression du médecin"
                };
            }
        }

        
        public async Task<ApiResponse<MedecinAnalytics>> GetAnalyticsAsync(Guid medecinId, DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                // Récupérer les rendez-vous sur la période
                var rdvs = await _firestoreRendezVousService.GetByMedecinIdAndDateAsync(medecinId, dateDebut);

                // Calculer les analytics
                var analytics = new MedecinAnalytics
                {
                    TotalConsultations = rdvs.Count(),
                    DureeMoyenneConsultation = (int)rdvs.Average(r => r.DureeMinutes),
                    TauxPonctualite = CalculerTauxPonctualite(rdvs),
                    // Ajouter d'autres calculs si nécessaire
                };

                return new ApiResponse<MedecinAnalytics>
                {
                    Success = true,
                    Data = analytics,
                    Message = "Analytics récupérées avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des analytics du médecin");
                return new ApiResponse<MedecinAnalytics>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des analytics"
                };
            }
        }

        public async Task<ApiResponse<bool>> SetDisponibilitesAsync(Guid medecinId, List<DisponibiliteDto> disponibilites)
        {
            try
            {
                // Logique pour mettre à jour les disponibilités dans Firestore
                // Implémentez selon vos besoins

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Disponibilités mises à jour avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour des disponibilités");
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la mise à jour des disponibilités"
                };
            }
        }

        private decimal CalculerTauxPonctualite(IEnumerable<RendezVous> rdvs)
        {
            if (!rdvs.Any()) return 100m;
            var rdvsPonctuels = rdvs.Count(r => r.DateHeure <= r.DateHeure.AddMinutes(5));
            return (decimal)rdvsPonctuels / rdvs.Count() * 100;
        }

        public async Task<ApiResponse<List<Medecin>>> GetByServiceAsync(Guid serviceId)
        {
            try
            {
                var medecins = await _medecinRepository.GetByServiceAsync(serviceId);
                return new ApiResponse<List<Medecin>>
                {
                    Success = true,
                    Data = medecins.ToList(),
                    Message = "Médecins récupérés avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des médecins par service");
                return new ApiResponse<List<Medecin>>
                {
                    Success = false,
                    Message = "Une erreur est survenue"
                };
            }
        }

        public async Task<ApiResponse<List<CreneauDisponible>>> GetDisponibilitesAsync(
    Guid medecinId,
    DateTime dateDebut,
    DateTime dateFin)
        {
            try
            {
                // 1. Vérifier que le médecin existe
                var medecin = await _medecinRepository.GetByIdAsync(medecinId);
                if (medecin == null)
                {
                    return new ApiResponse<List<CreneauDisponible>>
                    {
                        Success = false,
                        Message = "Médecin non trouvé"
                    };
                }

                // 2. Récupérer tous les rendez-vous du médecin sur la période
                var rdvs = await _firestoreRendezVousService.GetByMedecinIdAndDateAsync(medecinId, dateDebut.Date);

                // 3. Récupérer les disponibilités configurées depuis Firestore
                var dispoSnapshot = await _firestoreDb
                    .Collection("disponibilites_medecins")
                    .WhereEqualTo("medecinId", medecinId.ToString())
                    .WhereGreaterThanOrEqualTo("date", dateDebut.Date)
                    .WhereLessThanOrEqualTo("date", dateFin.Date)
                    .GetSnapshotAsync();

                var creneauxDisponibles = new List<CreneauDisponible>();

                foreach (var dispo in dispoSnapshot.Documents)
                {
                    var creneaux = dispo.GetValue<List<Dictionary<string, object>>>("creneaux");
                    foreach (var creneau in creneaux)
                    {
                        var debutCreneau = ((Timestamp)creneau["debut"]).ToDateTime();
                        var finCreneau = ((Timestamp)creneau["fin"]).ToDateTime();

                        // Vérifier si le créneau est déjà pris par un RDV
                        var estDisponible = (bool)creneau["disponible"] &&
                            !rdvs.Any(r =>
                                r.DateHeure >= debutCreneau && r.DateHeure < finCreneau ||
                                r.DateHeure.AddMinutes(r.DureeMinutes) > debutCreneau &&
                                 r.DateHeure.AddMinutes(r.DureeMinutes) <= finCreneau);

                        creneauxDisponibles.Add(new CreneauDisponible
                        {
                            DateDebut = debutCreneau,
                            DateFin = finCreneau,
                            DureeMinutes = (int)(finCreneau - debutCreneau).TotalMinutes,
                            EstDisponible = estDisponible
                        });
                    }
                }

                return new ApiResponse<List<CreneauDisponible>>
                {
                    Success = true,
                    Data = creneauxDisponibles.OrderBy(c => c.DateDebut).ToList(),
                    Message = $"{creneauxDisponibles.Count} créneaux trouvés"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des disponibilités du médecin {MedecinId}", medecinId);
                return new ApiResponse<List<CreneauDisponible>>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des disponibilités"
                };
            }
        }

        public async Task<ApiResponse<string>> UploadPhotoAsync(Guid medecinId, IFormFile photo)
        {
            try
            {
                _logger.LogInformation($"Début de l'upload de photo pour le médecin ID: {medecinId}");

                // Vérifier que le médecin existe
                var medecinResponse = await GetMedecinByIdAsync(medecinId);
                if (!medecinResponse.Success)
                {
                    _logger.LogWarning($"Médecin avec ID {medecinId} non trouvé lors de l'upload de photo");
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Médecin non trouvé"
                    };
                }

                // Vérifier le type de fichier (images seulement)
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(photo.ContentType.ToLower()))
                {
                    _logger.LogWarning($"Type de fichier non autorisé: {photo.ContentType}");
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Type de fichier non autorisé. Seuls les formats JPEG, PNG et GIF sont acceptés."
                    };
                }

                // Vérifier la taille du fichier (ex: max 5MB)
                if (photo.Length > 5 * 1024 * 1024)
                {
                    _logger.LogWarning($"Taille de fichier excessive: {photo.Length} bytes");
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "La taille du fichier dépasse la limite autorisée (5MB)."
                    };
                }

                // Générer un nom de fichier unique
                var fileName = $"{medecinId}_{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";

                // Chemin de stockage
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "medecins");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Sauvegarder le fichier
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(fileStream);
                }

                _logger.LogInformation($"Fichier sauvegardé: {filePath}");

                // URL relative pour accéder à l'image
                var photoUrl = $"/uploads/medecins/{fileName}";

                // Mettre à jour les informations de photo dans la base de données
                await _medecinRepository.UpdatePhotoAsync(
                    medecinId,
                    photoUrl,
                    photo.FileName,
                    photo.ContentType,
                    photo.Length
                );

                _logger.LogInformation($"Photo mise à jour avec succès pour le médecin ID: {medecinId}");

                return new ApiResponse<string>
                {
                    Success = true,
                    Data = photoUrl,
                    Message = "Photo uploadée avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'upload de la photo pour le médecin {MedecinId}", medecinId);
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de l'upload de la photo"
                };
            }
        }

        public async Task<ApiResponse<bool>> DeletePhotoAsync(Guid medecinId)
        {
            try
            {
                _logger.LogInformation($"Début de la suppression de photo pour le médecin ID: {medecinId}");

                // Récupérer le médecin pour obtenir l'URL de la photo
                var medecinResponse = await GetMedecinByIdAsync(medecinId);
                if (!medecinResponse.Success)
                {
                    _logger.LogWarning($"Médecin avec ID {medecinId} non trouvé lors de la suppression de photo");
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Médecin non trouvé"
                    };
                }

                var medecin = medecinResponse.Data;

                // Vérifier si le médecin a une photo
                if (string.IsNullOrEmpty(medecin.PhotoUrl))
                {
                    _logger.LogWarning($"Le médecin ID {medecinId} n'a pas de photo à supprimer");
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Ce médecin n'a pas de photo"
                    };
                }

                // Supprimer le fichier physique
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", medecin.PhotoUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation($"Fichier supprimé: {filePath}");
                }
                else
                {
                    _logger.LogWarning($"Fichier non trouvé lors de la suppression: {filePath}");
                }

                // Mettre à jour la base de données
                await _medecinRepository.RemovePhotoAsync(medecinId);

                _logger.LogInformation($"Photo supprimée avec succès pour le médecin ID: {medecinId}");

                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Photo supprimée avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la photo pour le médecin {MedecinId}", medecinId);
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la suppression de la photo"
                };
            }
        }

        public async Task<ApiResponse<Domain.Models.SQLserver.FileInfo>> GetPhotoInfoAsync(Guid medecinId)
        {
            try
            {
                _logger.LogInformation($"Récupération des informations de photo pour le médecin ID: {medecinId}");

                // Récupérer le médecin
                var medecinResponse = await GetMedecinByIdAsync(medecinId);
                if (!medecinResponse.Success)
                {
                    return new ApiResponse<Domain.Models.SQLserver.FileInfo>
                    {
                        Success = false,
                        Message = "Médecin non trouvé"
                    };
                }

                var medecin = medecinResponse.Data;

                // Vérifier si le médecin a une photo
                if (string.IsNullOrEmpty(medecin.PhotoUrl))
                {
                    return new ApiResponse<Domain.Models.SQLserver.FileInfo>
                    {
                        Success = false,
                        Message = "Ce médecin n'a pas de photo"
                    };
                }

                // Obtenir le chemin complet du fichier
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", medecin.PhotoUrl.TrimStart('/'));

                // Vérifier si le fichier existe
                if (!File.Exists(filePath))
                {
                    return new ApiResponse<Domain.Models.SQLserver.FileInfo>
                    {
                        Success = false,
                        Message = "Fichier photo introuvable sur le serveur"
                    };
                }

                // Créer un objet FileInfo pour retourner les informations
                var fileInfo = new Domain.Models.SQLserver.FileInfo
                {
                    FilePath = filePath,
                    FileName = medecin.PhotoNom,
                    ContentType = medecin.PhotoType,
                    FileSize = medecin.PhotoTaille ?? 0
                };

                return new ApiResponse<Domain.Models.SQLserver.FileInfo>
                {
                    Success = true,
                    Data = fileInfo,
                    Message = "Informations de photo récupérées avec succès"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des informations de photo pour le médecin {MedecinId}", medecinId);
                return new ApiResponse<Domain.Models.SQLserver.FileInfo>
                {
                    Success = false,
                    Message = "Une erreur est survenue lors de la récupération des informations de photo"
                };
            }
        }

    }
}
