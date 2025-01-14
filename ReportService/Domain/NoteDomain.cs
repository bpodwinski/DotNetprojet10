namespace ReportService.Domain
{
    public class NoteDomain
    {
        public string Id { get; set; }
        public int PatientId { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
