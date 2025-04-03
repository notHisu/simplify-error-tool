using System;
using System.Text.Json.Serialization;

namespace ErrorTool.Models
{
    public class VanStuckEntry
    {
        [JsonPropertyName("@timestamp")]
        public DateTime Timestamp { get; set; }
        
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        
        [JsonPropertyName("labels")]
        public VanStuckLabels? Labels { get; set; }
    }

    public class VanStuckLabels
    {
        [JsonPropertyName("Parcels")]
        public string? Parcels { get; set; }
        
    }
}
