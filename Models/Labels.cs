using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ErrorTool.Models
{
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
