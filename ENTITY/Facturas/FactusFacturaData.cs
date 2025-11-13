using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ENTITY.Facturas
{
    public class FactusFacturaData
    {
        [JsonPropertyName("bill")]
        public FactusBill Bill { get; set; } = null!;
    }
}
