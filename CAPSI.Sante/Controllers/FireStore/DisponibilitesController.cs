using CAPSI.Sante.Domain.Models.Firestore;
using CAPSI.Sante.Infrastructure.Firebase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CAPSI.Sante.API.Controllers.FireStore
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DisponibilitesController : ControllerBase
    {
        private readonly DisponibiliteService _disponibiliteService;

        public DisponibilitesController(DisponibiliteService disponibiliteService)
        {
            _disponibiliteService = disponibiliteService;
        }

        // POST: api/Disponibilites
        [HttpPost]
        public async Task<ActionResult<string>> SetDisponibilites(Disponibilite disponibilite)
        {
            var id = await _disponibiliteService.SetDisponibilitesAsync(disponibilite);
            return CreatedAtAction(nameof(GetDisponibilitesDate),
                new { medecinId = disponibilite.MedecinId, date = disponibilite.Date.ToString("yyyy-MM-dd") }, id);
        }

        // GET: api/Disponibilites/{medecinId}/{dateDebut}/{dateFin}
        [HttpGet("{medecinId}/{dateDebut}/{dateFin}")]
        public async Task<ActionResult<List<Disponibilite>>> GetCreneauxDisponibles(
            string medecinId, DateTime dateDebut, DateTime dateFin)
        {
            var disponibilites = await _disponibiliteService.GetCreneauxDisponiblesAsync(medecinId, dateDebut, dateFin);
            return disponibilites;
        }

        // GET: api/Disponibilites/{medecinId}/{date}
        [HttpGet("{medecinId}/{date}")]
        public async Task<ActionResult<Disponibilite>> GetDisponibilitesDate(string medecinId, DateTime date)
        {
            var disponibilite = await _disponibiliteService.GetDisponibilitesDateAsync(medecinId, date);
            if (disponibilite == null)
            {
                return NotFound();
            }
            return disponibilite;
        }

        // PUT: api/Disponibilites/{disponibiliteId}/creneau
        [HttpPut("{disponibiliteId}/creneau")]
        public async Task<IActionResult> UpdateCreneau(string disponibiliteId, [FromBody] Creneau creneau)
        {
            await _disponibiliteService.UpdateCreneauAsync(disponibiliteId, creneau);
            return NoContent();
        }
    }
}
