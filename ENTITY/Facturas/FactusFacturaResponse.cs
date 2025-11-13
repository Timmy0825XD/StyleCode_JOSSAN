using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ENTITY.Facturas
{
    public class FactusFacturaResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;

        [JsonPropertyName("data")]
        public FactusFacturaData? Data { get; set; }
    }
}
