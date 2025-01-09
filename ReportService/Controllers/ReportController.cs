using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReportService.DTOs;
using ReportService.Services;

namespace ReportService.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportController(IReportService reportService, ILogger<ReportController> logger) : ControllerBase
    {
        private readonly IReportService _reportService = reportService;
        private readonly ILogger<ReportController> _logger = logger;

        /// <summary>
        /// Retrieves a diabetes risk report for a specific patient by ID.
        /// </summary>
        /// <param name="id">The ID of the patient</param>
        /// <returns>A ReportDTO containing the diabetes risk level and trigger terms</returns>
        /// <response code="200">Returns the diabetes risk report</response>
        /// <response code="404">If the patient with the specified ID is not found</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpGet("diabete/{id}")]
        [Authorize(policy: "User")]
        [ProducesResponseType(typeof(ReportDTO), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDiabete([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Fetching diabetes risk report for patient with ID {Id}.", id);

                var report = await _reportService.GetDiabeteByPatientId(id);

                if (report is not null)
                {
                    return Ok(report);
                }

                _logger.LogWarning("No diabetes risk report found for patient with ID {Id}.", id);
                return NotFound($"Patient with ID {id} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the diabetes risk report for patient with ID {Id}.", id);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
