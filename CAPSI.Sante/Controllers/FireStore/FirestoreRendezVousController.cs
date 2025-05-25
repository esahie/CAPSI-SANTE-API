using CAPSI.Sante.Domain.Models.Firestore;
using CAPSI.Sante.Infrastructure.Firebase.Services;

//using CAPSI.Sante.Infrastructure.Firebase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CAPSI.Sante.API.Controllers.FireStore
{
    [Route("api/firestore/rendez-vous")]
    [ApiController]
    //[Authorize]
    public class FirestoreRendezVousController : ControllerBase
    {
        private readonly FirestoreRendezVous _rendezVousService;

        public FirestoreRendezVousController(FirestoreRendezVous rendezVousService)
        {
            _rendezVousService = rendezVousService;
        }

        // GET: api/firestore/rendez-vous/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RendezVous>> GetRendezVous(string id)
        {
            var rdv = await _rendezVousService.GetRendezVousAsync(id);
            if (rdv == null)
            {
                return NotFound();
            }
            return rdv;
        }

        // GET: api/firestore/rendez-vous/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<List<RendezVous>>> GetRendezVousPatient(string patientId)
        {
            var rdvs = await _rendezVousService.GetRendezVousPatientAsync(patientId);
            return rdvs;
        }

        // GET: api/firestore/rendez-vous/medecin/{medecinId}
        [HttpGet("medecin/{medecinId}")]
        public async Task<ActionResult<List<RendezVous>>> GetRendezVousMedecin(string medecinId)
        {
            var rdvs = await _rendezVousService.GetRendezVousMedecinAsync(medecinId);
            return rdvs;
        }

        // POST: api/firestore/rendez-vous
        [HttpPost]
        public async Task<ActionResult<string>> CreerRendezVous(RendezVous rdv)
        {
            var rdvId = await _rendezVousService.CreerRendezVousAsync(rdv);
            return CreatedAtAction(nameof(GetRendezVous), new { id = rdvId }, rdvId);
        }

        // PUT: api/firestore/rendez-vous/{id}/statut
        [HttpPut("{id}/statut")]
        public async Task<IActionResult> UpdateStatut(string id, [FromBody] string nouveauStatut)
        {
            await _rendezVousService.UpdateStatutAsync(id, nouveauStatut);
            return NoContent();
        }

        // PUT: api/firestore/rendez-vous/{id}/annuler
        [HttpPut("{id}/annuler")]
        public async Task<IActionResult> AnnulerRendezVous(string id, [FromBody] string motif = null)
        {
            await _rendezVousService.AnnulerRendezVousAsync(id, motif);
            return NoContent();
        }
    }
}
