using Microsoft.ML;
using ReportService.Models;
using ReportService.Utils;
using System.Diagnostics;

namespace ReportService.Services
{
    /// <summary>
    /// The MachineLearningService class is responsible for managing the training and prediction of ML.NET models.
    /// </summary>
    public class MachineLearningService : IMachineLearningService
    {
        private readonly PredictionEngine<MachineLearningInputModel, TriggerPrediction> _predictionEngine;
        private readonly string _modelPath;
        private readonly ILogger<MachineLearningService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MachineLearningService"/> class.
        /// </summary>
        /// <param name="modelTrainer">The <see cref="IModelTrainer"/> used to train the model if not found.</param>
        /// <param name="configuration">Application configuration containing the model path.</param>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <exception cref="ArgumentNullException">Thrown if required dependencies are null or missing.</exception>
        public MachineLearningService(IModelTrainer modelTrainer, IConfiguration configuration, ILogger<MachineLearningService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modelPath = configuration["MLModelPath"] ?? throw new ArgumentNullException("MLModelPath configuration is missing");


            if (!File.Exists(_modelPath))
            {
                _logger.LogWarning($"ML.NET model not found at path: {_modelPath}. Training a new model...");

                var stopwatch = Stopwatch.StartNew();
                modelTrainer.TrainModel("AI_model_training_data.csv");
                stopwatch.Stop();

                _logger.LogInformation($"Model training completed in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
            }

            var mlContext = new MLContext();
            var model = mlContext.Model.Load(_modelPath, out var _);
            _predictionEngine = mlContext.Model.CreatePredictionEngine<MachineLearningInputModel, TriggerPrediction>(model);
        }

        /// <summary>
        /// Identifies triggers in the provided input data.
        /// </summary>
        /// <param name="input">The input model containing structured and textual data.</param>
        /// <returns>A list of detected trigger terms.</returns>
        public List<string> IdentifyTriggers(MachineLearningInputModel input)
        {
            return MachineLearningUtils.IdentifyTriggers(input);
        }

        /// <summary>
        /// Calculates the risk level based on the input data and the number of detected triggers.
        /// </summary>
        /// <param name="input">The input model containing structured and textual data.</param>
        /// <param name="triggerCount">The number of detected triggers.</param>
        /// <returns>A string representing the calculated risk level.</returns>
        public string CalculateRiskLevel(MachineLearningInputModel input, int triggerCount)
        {
            return MachineLearningUtils.CalculateRiskLevel(input, triggerCount);
        }
    }
}
