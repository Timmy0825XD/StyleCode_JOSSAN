using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ENTITY.Facturas
{
    public class FactusBill
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; } = null!;

        [JsonPropertyName("reference_code")]
        public string ReferenceCode { get; set; } = null!;

        [JsonPropertyName("cufe")]
        public string Cufe { get; set; } = null!;

        [JsonPropertyName("qr")]
        public string Qr { get; set; } = null!; // URL del QR DIAN

        [JsonPropertyName("validated")]
        public string? Validated { get; set; }

        [JsonPropertyName("gross_value")]
        public string? GrossValue { get; set; }

        [JsonPropertyName("tax_amount")]
        public string? TaxAmount { get; set; }

        [JsonPropertyName("total")]
        public string? Total { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }
    }
}
