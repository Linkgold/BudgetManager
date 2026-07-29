using System.Text.Json.Serialization;

namespace UI.Models.API
{
    public class ApiValidationError
    {
        [JsonPropertyName("property")]
        public string? Property { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("attemptedValue")]
        public object? AttemptedValue { get; set; }
    }
}