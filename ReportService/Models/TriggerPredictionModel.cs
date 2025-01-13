namespace ReportService.Models
{
    /// <summary>
    /// Represents the prediction output of the ML model
    /// </summary>
    public class TriggerPrediction
    {
        public bool PredictedLabel { get; set; }
        public float Probability { get; set; }
        public float Score { get; set; }
    }
}
