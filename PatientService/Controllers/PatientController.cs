using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientService.DTOs;
using PatientService.Services;

namespace PatientService.Controllers
{
    [ApiController]
    [Route("api/patients")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly ILogger<PatientController> _logger;

        public PatientController(IPatientService patientService, ILogger<PatientController> logger)
        {
            _patientService = patientService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all Patient items.
        /// </summary>
        /// <remarks>
        /// This method returns all Patient entities in the system.
        /// </remarks>
        /// <returns>A list of PatientDTOs</returns>
        /// <response code="200">Returns the list of PatientDTOs</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpGet]
        [Authorize(policy: "User")]
        [ProducesResponseType(typeof(List<PatientDTO>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Fetching all Patient items.");
                var patients = await _patientService.GetAll();
                return Ok(patients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching Patient items.");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Retrieves a specific Patient by ID.
        /// </summary>
        /// <param name="id">The ID of the Patient to retrieve</param>
        /// <returns>A PatientDTO corresponding to the ID</returns>
        /// <response code="200">Returns the PatientDTO</response>
        /// <response code="404">If the Patient with the specified ID is not found</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpGet]
        [Route("{id}")]
        [Authorize(policy: "User")]
        [ProducesResponseType(typeof(PatientDTO), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Fetching Patient item with ID {Id}.", id);
                var patient = await _patientService.GetById(id);

                if (patient is not null)
                {
                    return Ok(patient);
                }

                _logger.LogWarning("Patient item with ID {Id} not found.", id);
                return NotFound($"Patient with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching Patient item with ID {Id}.", id);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Creates a new Patient.
        /// </summary>
        /// <param name="dto">The PatientDTO object to create</param>
        /// <returns>The newly created PatientDTO</returns>
        /// <response code="201">Returns the newly created PatientDTO</response>
        /// <response code="400">If the model is invalid</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpPost]
        [Authorize(policy: "Admin")]
        [ProducesResponseType(typeof(PatientDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Create([FromBody] PatientDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for creating a Patient.");
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Creating a new Patient item.");
                var createdPatient = await _patientService.Create(dto);
                return CreatedAtAction(nameof(GetById), new { id = createdPatient.Id }, createdPatient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a Patient item.");
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Updates a specific Patient.
        /// </summary>
        /// <param name="id">The ID of the Patient to update</param>
        /// <param name="dto">The PatientDTO object with updated values</param>
        /// <returns>The updated PatientDTO</returns>
        /// <response code="200">Returns the updated PatientDTO</response>
        /// <response code="404">If the Patient with the specified ID is not found</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpPut]
        [Route("{id}")]
        [Authorize(policy: "Admin")]
        [ProducesResponseType(typeof(PatientDTO), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PatientDTO dto)
        {
            try
            {
                _logger.LogInformation("Updating Patient item with ID {Id}.", id);
                var updatedPatient = await _patientService.Update(id, dto);
                if (updatedPatient is not null)
                {
                    return Ok(updatedPatient);
                }
                _logger.LogWarning("Patient item with ID {Id} not found for update.", id);
                return NotFound($"Patient with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating Patient item with ID {Id}.", id);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Deletes a specific Patient by ID.
        /// </summary>
        /// <param name="id">The ID of the Patient to delete</param>
        /// <response code="204">If the Patient is successfully deleted</response>
        /// <response code="404">If the Patient with the specified ID is not found</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpDelete]
        [Route("{id}")]
        [Authorize(policy: "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Deleting Patient item with ID {Id}.", id);
                var deletedPatient = await _patientService.DeleteById(id);
                if (deletedPatient is not null)
                {
                    return NoContent();
                }
                _logger.LogWarning("Patient item with ID {Id} not found for deletion.", id);
                return NotFound($"Patient with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting Patient item with ID {Id}.", id);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
