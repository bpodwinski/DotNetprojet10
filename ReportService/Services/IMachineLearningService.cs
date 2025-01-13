using ReportService.Models;

namespace ReportService.Services
{
    /// <summary>
    /// Interface defining the contract for machine learning operations.
    /// </summary>
    public interface IMachineLearningService
    {
        /// <summary>
        /// Identifies triggers from structured and textual patient data.
        /// </summary>
        /// <param name="input">The input model containing patient data.</param>
        /// <returns>A list of detected triggers.</returns>
        List<string> IdentifyTriggers(MachineLearningInputModel input);

        /// <summary>
        /// Calculates the risk level based on input data and the number of detected triggers.
        /// </summary>
        /// <param name="input">The input model containing patient data.</param>
        /// <param name="triggerCount">The number of detected triggers.</param>
        /// <returns>A string representing the calculated risk level.</returns>
        string CalculateRiskLevel(MachineLearningInputModel input, int triggerCount);
    }
}
