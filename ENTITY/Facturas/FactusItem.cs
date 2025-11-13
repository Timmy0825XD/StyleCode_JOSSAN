using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ENTITY.Facturas
{
    public class FactusItem
    {
        [JsonPropertyName("code_reference")]
        public string CodeReference { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("discount_rate")]
        public decimal DiscountRate { get; set; } = 0;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("tax_rate")]
        public string TaxRate { get; set; } = "19.00";

        [JsonPropertyName("unit_measure_id")]
        public int UnitMeasureId { get; set; } = 70;

        [JsonPropertyName("standard_code_id")]
        public int StandardCodeId { get; set; } = 1;

        [JsonPropertyName("is_excluded")]
        public int IsExcluded { get; set; } = 0;

        [JsonPropertyName("tribute_id")]
        public int TributeId { get; set; } = 1;
    }
}
