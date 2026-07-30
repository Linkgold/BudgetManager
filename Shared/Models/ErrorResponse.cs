namespace Shared.Models
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public string? Detail { get; set; }
        public DateTime Timestamp { get; set; }
    }
}