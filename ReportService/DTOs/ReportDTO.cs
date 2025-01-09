using Swashbuckle.AspNetCore.Annotations;

namespace ReportService.DTOs
{
    /// <summary>
    /// Data Transfer Object representing a diabetes risk report.
    /// </summary>
    public class ReportDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier of the patient.
        /// </summary>
        [SwaggerSchema(ReadOnly = true)]
        public int PatientId { get; set; }

        /// <summary>
        /// Gets or sets the risk level of diabetes for the patient.
        /// </summary>
        public string RiskLevel { get; set; }

        /// <summary>
        /// Gets or sets the list of trigger terms found in the patient's medical notes.
        /// </summary>
        public List<string> TriggerTerms { get; set; }
    }
}
