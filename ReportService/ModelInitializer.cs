namespace ReportService
{
    public class ModelInitializer
    {
        private readonly ML _triggerAnalyzer;
        private readonly ILogger<ModelInitializer> _logger;

        public ModelInitializer(ML triggerAnalyzer, ILogger<ModelInitializer> logger)
        {
            _triggerAnalyzer = triggerAnalyzer;
            _logger = logger;
        }

        public void Initialize()
        {
            _logger.LogInformation("Vérification du modèle ML.NET au démarrage de l'application...");
        }
    }
}
