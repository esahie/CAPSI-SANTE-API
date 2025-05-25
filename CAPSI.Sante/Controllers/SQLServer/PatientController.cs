using CAPSI.Sante.Application.DTOs.Patient;
using CAPSI.Sante.Application.Services.SQLserver.Interfaces;
using CAPSI.Sante.Common;
using CAPSI.Sante.Domain.Models.SQLserver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CAPSI.Sante.API.Controllers.SQLServer
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : BaseApiController
    {
        private readonly IPatientService _patientService;
        private readonly ILogger<PatientController> _logger;

        public PatientController(IPatientService patientService, ILogger<PatientController> logger)
        {
            _patientService = patientService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Patient>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<Patient>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<Patient>>> CreatePatient([FromBody] CreatePatientDto dto)
        {
            var response = await _patientService.CreatePatientAsync(dto);
            if (!response.Success)
                return BadRequest(response);

            return CreatedAtAction(nameof(GetPatient), new { id = response.Data.Id }, response);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Patient>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Patient>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Patient>>> GetPatient(Guid id)
        {
            var response = await _patientService.GetPatientByIdAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

       
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Patient>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Patient>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Patient>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<Patient>>> UpdatePatient(
            Guid id,
            [FromBody] UpdatePatientDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new ApiResponse<Patient> { Success = false, Message = "ID incohérent" });

            var response = await _patientService.UpdatePatientAsync(dto);
            if (!response.Success)
                return response.Message.Contains("non trouvé") ? NotFound(response) : BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePatient(Guid id)
        {
            var response = await _patientService.DeletePatientAsync(id);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiResponse<List<Patient>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<Patient>>>> SearchPatients([FromQuery] string term)
        {
            var response = await _patientService.SearchPatientsAsync(term);
            return Ok(response);
        }

        [HttpPut("{id}/deactivate")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivatePatient(Guid id)
        {
            var response = await _patientService.DeactivatePatientAsync(id);
            if (!response.Success)
                return response.Message.Contains("non trouvé") ? NotFound(response) : BadRequest(response);

            return Ok(response);
        }

        [HttpPut("{id}/reactivate")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> ReactivatePatient(Guid id)
        {
            var response = await _patientService.ReactivatePatientAsync(id);
            if (!response.Success)
                return response.Message.Contains("non trouvé") ? NotFound(response) : BadRequest(response);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<Patient>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<Patient>>>> GetPatients(
            [FromQuery] string searchTerm = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeInactive = false)
        {
            var response = await _patientService.GetPatientsAsync(searchTerm, page, pageSize, includeInactive);
            return Ok(response);
        }
    }
}
