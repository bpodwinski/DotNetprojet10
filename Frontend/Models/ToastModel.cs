namespace Frontend.Models
{
    public class ToastModel
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public ToastSeverity Severity { get; set; } = ToastSeverity.Info;
    }

    public enum ToastSeverity
    {
        Info,
        Success,
        Warning,
        Error
    }
}
