using Microsoft.ML.Transforms;
using ReportService.Models;
using ReportService.Utils;

namespace ReportService.Services
{
    /// <summary>
    /// Provides custom mapping for identifying triggers and calculating risk levels
    /// during ML.NET pipeline processing.
    /// </summary>
    [CustomMappingFactoryAttribute("CalculateTriggersAndRisk")]
    public class CalculateTriggersAndRiskMapping : CustomMappingFactory<MachineLearningInputModel, MachineLearningOutputModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculateTriggersAndRiskMapping"/> class.
        /// Required for dynamic instantiation by ML.NET.
        /// </summary>
        public CalculateTriggersAndRiskMapping() { }

        /// <summary>
        /// Defines the custom mapping logic for detecting triggers and calculating
        /// risk levels based on the provided input data.
        /// </summary>
        /// <returns>
        /// An <see cref="Action{T1, T2}"/> that processes the input data
        /// (<see cref="MachineLearningInputModel"/>) and populates the output
        /// (<see cref="MachineLearningOutputModel"/>) with the calculated results.
        /// </returns>
        public override Action<MachineLearningInputModel, MachineLearningOutputModel> GetMapping()
        {
            return (input, output) =>
            {
                // Identify triggers in the input data using utility methods
                var detectedTriggers = MachineLearningUtils.IdentifyTriggers(input);

                // Populate output with detected triggers and risk level
                output.Triggers = string.Join(",", detectedTriggers);
                output.TriggerCount = detectedTriggers.Count;
                output.RiskLevel = MachineLearningUtils.CalculateRiskLevel(input, output.TriggerCount);
            };
        }
    }
}
