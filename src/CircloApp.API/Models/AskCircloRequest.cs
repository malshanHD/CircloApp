namespace CircloApp.API.Models
{
    public class AskCircloRequest
    {
        public Guid EventId { get; set; }
        public string Question { get; set; } = string.Empty;
    }
}
