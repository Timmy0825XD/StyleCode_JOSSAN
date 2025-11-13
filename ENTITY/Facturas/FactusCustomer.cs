using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ENTITY.Facturas
{
    public class FactusCustomer
    {
        [JsonPropertyName("identification_document_id")]
        public int IdentificationDocumentId { get; set; } = 3; // 3 = Cédula

        [JsonPropertyName("identification")]
        public string Identification { get; set; } = null!;

        [JsonPropertyName("dv")]
        public string? Dv { get; set; }

        [JsonPropertyName("company")]
        public string? Company { get; set; }

        [JsonPropertyName("names")]
        public string Names { get; set; } = null!;

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("legal_organization_id")]
        public string LegalOrganizationId { get; set; } = "2"; // 2 = Persona Natural

        [JsonPropertyName("tribute_id")]
        public string TributeId { get; set; } = "21"; // 21 = No aplica

        [JsonPropertyName("municipality_id")]
        public int? MunicipalityId { get; set; }
    }
}
