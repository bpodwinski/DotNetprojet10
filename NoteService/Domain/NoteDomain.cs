namespace NoteService.Domain
{
    public class NoteDomain
    {
        public string Id { get; set; }
        public required int PatientId { get; set; }
        public required string Note { get; set; }
    }
}
