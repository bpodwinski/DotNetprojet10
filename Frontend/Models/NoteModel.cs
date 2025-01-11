namespace Frontend.Models
{
    public class NoteModel
    {
        public string Id { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public string Note { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string LocalDate
        {
            get
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris");
                return TimeZoneInfo.ConvertTimeFromUtc(Date, timeZone).ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
    }
}
