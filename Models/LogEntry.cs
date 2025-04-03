using System;
using System.Text.Json.Serialization;

namespace ErrorTool.Models
{
    public class LogEntry
    {
        [JsonPropertyName("@timestamp")]
        public DateTime Timestamp { get; set; }
        
        [JsonPropertyName("log.level")]
        public string? LogLevel { get; set; }
        
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        
        [JsonPropertyName("error")]
        public ErrorInfo? Error { get; set; }
        
        [JsonPropertyName("labels")]
        public Labels? Labels { get; set; }

        // Flattened properties for DataGridView
        [JsonIgnore]
        public string? ErrorMessage => Error?.Message;

        [JsonIgnore]
        public string? Sender => Labels?.Sender;

        [JsonIgnore]
        public string? InternalTesting => Labels?.InternalTesting;

        [JsonIgnore]
        public string? Parcels => Labels?.Parcels;
    }

    public class ErrorInfo
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public class Labels
    {
        [JsonPropertyName("Sender")]
        public string? Sender { get; set; }
        
        [JsonPropertyName("InternalTesting")]
        public string? InternalTesting { get; set; }
        [JsonPropertyName("Parcels")]
        public string? Parcels { get; set; }
    }
}