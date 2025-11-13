using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ENTITY.Facturas
{
    public class FactusTokenRequest
    {
        [JsonPropertyName("grant_type")]
        public string GrantType { get; set; } = "password";

        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = null!;

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; } = null!;

        [JsonPropertyName("username")]
        public string Username { get; set; } = null!;

        [JsonPropertyName("password")]
        public string Password { get; set; } = null!;
    }
}
