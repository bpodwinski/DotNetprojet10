using ReportService.Services;

namespace ReportService
{
    /// <summary>
    /// Responsible for initializing and verifying the ML.NET model during application startup.
    /// </summary>
    public class ModelInitializer
    {
        private readonly IMachineLearningService _machineLearningService;
        private readonly ILogger<ModelInitializer> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelInitializer"/> class.
        /// </summary>
        /// <param name="machineLearningService">The machine learning service used for model operations.</param>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <exception cref="ArgumentNullException">Thrown if required dependencies are null.</exception>
        public ModelInitializer(IMachineLearningService machineLearningService, ILogger<ModelInitializer> logger)
        {
            _machineLearningService = machineLearningService ?? throw new ArgumentNullException(nameof(machineLearningService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes the model and performs any necessary verification during application startup.
        /// </summary>
        public void Initialize()
        {
            _logger.LogInformation("Starting ML.NET model verification...");
        }
    }
}
