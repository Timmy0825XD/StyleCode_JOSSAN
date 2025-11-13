using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ENTITY.Facturas
{
    public class FactusFacturaRequest
    {
        [JsonPropertyName("document")]
        public string Document { get; set; } = "01";

        [JsonPropertyName("reference_code")]
        public string ReferenceCode { get; set; } = null!;

        [JsonPropertyName("observation")]
        public string? Observation { get; set; }

        [JsonPropertyName("payment_method_code")]
        public string PaymentMethodCode { get; set; } = "10";

        [JsonPropertyName("customer")]
        public FactusCustomer Customer { get; set; } = null!;

        [JsonPropertyName("items")]
        public List<FactusItem> Items { get; set; } = new();
    }
}
