namespace Frontend.Models
{
    public class ValidationErrorModel
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; } = new();
        public string TraceId { get; set; } = string.Empty;
    }

}
