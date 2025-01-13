using Microsoft.ML;
using Microsoft.ML.Data;
using ReportService.Models;
using ReportService.Utils;

namespace ReportService.Services
{
    /// <summary>
    /// Class responsible for training an ML.NET model.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ModelTrainer"/> class.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if any required dependency is null.</exception>
    public class ModelTrainer : IModelTrainer
    {
        private readonly string _modelPath;
        private readonly ILogger<ModelTrainer> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelTrainer"/> class.
        /// </summary>
        /// <param name="modelPath">The path where the trained model will be saved.</param>
        /// <param name="logger">An instance of the logger to record information and errors.</param>
        public ModelTrainer(IConfiguration configuration, ILogger<ModelTrainer> logger)
        {
            _modelPath = configuration["MLModelPath"] ?? throw new ArgumentNullException("MLModelPath configuration is missing");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Trains an ML.NET model using the specified training data and saves the model to the provided path.
        /// </summary>
        /// <param name="csvPath">The path to the CSV file containing the training data.</param>
        /// <exception cref="FileNotFoundException">Thrown if the training data CSV file is not found.</exception>
        public void TrainModel(string csvPath)
        {
            var mlContext = new MLContext();

            try
            {
                if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
                {
                    _logger.LogError($"The training data file was not found: {csvPath}");
                    throw new FileNotFoundException($"The training data file was not found: {csvPath}");
                }

                var trainingDataView = mlContext.Data.LoadFromTextFile<MachineLearningInputModel>(
                    path: csvPath,
                    hasHeader: true,
                    separatorChar: ';');

                // Define pipeline
                var pipeline = mlContext.Transforms
                    .CustomMapping<MachineLearningInputModel, MachineLearningOutputModel>(
                        (input, output) =>
                        {
                            output.TriggerCount = MachineLearningUtils.IdentifyTriggers(input).Count;
                            output.RiskLevel = MachineLearningUtils.CalculateRiskLevel(input, output.TriggerCount);
                        },
                        contractName: "CalculateTriggersAndRisk")
                    .Append(mlContext.Transforms.Conversion.ConvertType(
                        outputColumnName: "TriggerCountFloat",
                        inputColumnName: "TriggerCount",
                        outputKind: DataKind.Single))
                    .Append(mlContext.Transforms.Concatenate("Features", "Age", "TriggerCountFloat"))
                    .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                    .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                        new Microsoft.ML.Trainers.SdcaLogisticRegressionBinaryTrainer.Options
                        {
                            LabelColumnName = "Label",
                            FeatureColumnName = "Features",
                            MaximumNumberOfIterations = 10000,
                            ConvergenceTolerance = 1e-4F,
                            BiasLearningRate = 0.01f
                        }));

                // Train model
                var model = pipeline.Fit(trainingDataView);

                // Save model
                mlContext.ComponentCatalog.RegisterAssembly(typeof(CalculateTriggersAndRiskMapping).Assembly);
                mlContext.Model.Save(model, trainingDataView.Schema, _modelPath);
                _logger.LogInformation($"Model trained and saved to: {_modelPath}");
            }
            catch (FileNotFoundException fnfEx)
            {
                _logger.LogError($"File not found: {fnfEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred during model training: {ex.Message}");
                throw new InvalidOperationException("Model training failed due to an unexpected error.", ex);
            }
        }
    }
}
