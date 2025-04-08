using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace ErrorTool.Models
{
    public class ParcelInfo
    {
        [JsonPropertyName("parcelIds")]
        public List<long> ParcelIds { get; set; } = new List<long>();

        [JsonPropertyName("mailboxId")]
        public string? MailboxId { get; set; }

        [JsonPropertyName("connectionName")]
        public string? ConnectionName { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }
        
        [JsonIgnore]
        public int ParcelCount => ParcelIds?.Count ?? 0;
        
        [JsonIgnore]
        public string ParcelIdsSummary => ParcelIds.Count > 0 ? 
            $"{ParcelIds[0]}" + (ParcelIds.Count > 1 ? $" and {ParcelIds.Count-1} more" : "") : 
            "No parcels";
    }

    public class VanStuckResponse
    {
        [JsonPropertyName("parcelInfoList")]
        public List<ParcelInfo> ParcelInfoList { get; set; } = new List<ParcelInfo>();
        
        [JsonIgnore]
        public int TotalParcelCount => ParcelInfoList.Sum(p => p.ParcelCount);
    }
}
