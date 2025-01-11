namespace Frontend.Models
{
    public class DiabeteModel
    {
        public int PatientId { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public List<string> TriggerTerms { get; set; } = new List<string>();
    }
}
