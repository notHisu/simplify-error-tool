using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ErrorTool.Models
{
    public class VanStuckViewModel
    {
        public DateTime Timestamp { get; set; }
        public string? MailboxId { get; set; }
        public string? ConnectionName { get; set; }
        public string? UserName { get; set; }
        public int? ParcelCount { get; set; }
        public string? ParcelIds { get; set; }
        public List<long> RawParcelIds { get; set; } = new List<long>();

    }
}
