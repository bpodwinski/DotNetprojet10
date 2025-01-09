namespace ReportService.Domain
{
    public class ReportDomain
    {
        public required int PatientId { get; set; }
        public string RiskLevel { get; set; }
        public List<string> TriggerTerms { get; set; }
    }
}
