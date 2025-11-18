using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GUI.Services
{
    /// <summary>
    /// Servicio para comunicación con la API de Virtual Try-On Webcam
    /// </summary>
    public class WebcamTryOnService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "http://localhost:8000";

        public WebcamTryOnService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BASE_URL);
        }

        /// <summary>
        /// Verifica si la API está disponible
        /// </summary>
        public async Task<bool> IsApiHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene las estadísticas del servidor
        /// </summary>
        public async Task<WebcamStatsResponse?> GetStatsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<WebcamStatsResponse>("/stats");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo estadísticas: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene la lista de items disponibles (gafas o camisas)
        /// </summary>
        public async Task<List<string>> GetItemsAsync(string mode)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ItemsResponse>($"/items/{mode}");
                return response?.Items ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo items: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Sube un nuevo item (gafas o camisa)
        /// </summary>
        public async Task<bool> UploadItemAsync(string mode, string name, byte[] imageData)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(imageData), "file", $"{name}.png");

                var response = await _httpClient.PostAsync($"/upload/{mode}?name={Uri.EscapeDataString(name)}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error subiendo item: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Procesa una imagen individual
        /// </summary>
        public async Task<byte[]?> ProcessSingleImageAsync(string mode, string itemName, byte[] imageData)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(imageData), "file", "image.jpg");

                var response = await _httpClient.PostAsync(
                    $"/process-image?mode={mode}&item_name={Uri.EscapeDataString(itemName)}",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando imagen: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Resetea las estadísticas del servidor
        /// </summary>
        public async Task<bool> ResetStatsAsync()
        {
            try
            {
                var response = await _httpClient.DeleteAsync("/reset");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    // ==================== Modelos ====================

    public class WebcamStatsResponse
    {
        [JsonPropertyName("stats")]
        public WebcamStats Stats { get; set; } = new();

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = "";
    }

    public class WebcamStats
    {
        [JsonPropertyName("frames_processed")]
        public int FramesProcessed { get; set; }

        [JsonPropertyName("glasses_applied")]
        public int GlassesApplied { get; set; }

        [JsonPropertyName("shirts_applied")]
        public int ShirtsApplied { get; set; }

        [JsonPropertyName("total_sessions")]
        public int TotalSessions { get; set; }

        [JsonPropertyName("errors")]
        public int Errors { get; set; }
    }

    public class ItemsResponse
    {
        [JsonPropertyName("items")]
        public List<string> Items { get; set; } = new();
    }

    public class UploadResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class HealthResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = "";

        [JsonPropertyName("mediapipe")]
        public string MediaPipe { get; set; } = "";

        [JsonPropertyName("glasses_count")]
        public int GlassesCount { get; set; }

        [JsonPropertyName("shirts_count")]
        public int ShirtsCount { get; set; }
    }
}