using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace GUI.Services
{
    public class VirtualTryOnApp
    {
        /// <summary>
        /// Modelo para las estadísticas de uso
        /// </summary>
        public class TryOnStats
        {
            public string Date { get; set; }
            public StatsData Stats { get; set; }
        }

        public class StatsData
        {
            public int Total { get; set; }
            public int Successful { get; set; }
            public int Failed { get; set; }
            public int Good_Quality { get; set; }
            public Dictionary<string, int> By_Type { get; set; }
        }

        /// <summary>
        /// Tipos de prendas soportados
        /// </summary>
        public enum GarmentType
        {
            Upper,  // Camisas, blusas, tops
            Lower,  // Pantalones, faldas
            Dress   // Vestidos, monos
        }

        /// <summary>
        /// Información sobre un tipo de prenda
        /// </summary>
        public class GarmentTypeInfo
        {
            public string Name { get; set; }
            public List<string> Examples { get; set; }
            public string Recommendation { get; set; }
            public string Tip { get; set; }
        }

        /// <summary>
        /// Servicio para consumir la API de Virtual Try-On
        /// </summary>
        public class VirtualTryOnService
        {
            private readonly HttpClient _httpClient;
            private readonly string _apiBaseUrl;

            public VirtualTryOnService(HttpClient httpClient)
            {
                _httpClient = httpClient;
                _apiBaseUrl = "http://localhost:8000"; // Cambia esto si tu API está en otra URL
                _httpClient.Timeout = TimeSpan.FromMinutes(5); // Try-on puede tardar
            }

            /// <summary>
            /// Verifica si la API está disponible
            /// </summary>
            public async Task<bool> IsApiHealthyAsync()
            {
                try
                {
                    var response = await _httpClient.GetAsync($"{_apiBaseUrl}/health");
                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Obtiene las estadísticas de uso del día
            /// </summary>
            public async Task<TryOnStats> GetStatsAsync()
            {
                try
                {
                    var response = await _httpClient.GetAsync($"{_apiBaseUrl}/stats");
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadFromJsonAsync<TryOnStats>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error obteniendo estadísticas: {ex.Message}");
                    return null;
                }
            }

            /// <summary>
            /// Convierte el enum GarmentType a string para la API
            /// </summary>
            private string GarmentTypeToString(GarmentType type)
            {
                return type switch
                {
                    GarmentType.Upper => "upper",
                    GarmentType.Lower => "lower",
                    GarmentType.Dress => "dress",
                    _ => "upper"
                };
            }

            /// <summary>
            /// Procesa el virtual try-on
            /// </summary>
            /// <param name="personImageStream">Stream de la imagen de la persona</param>
            /// <param name="personFileName">Nombre del archivo de la persona</param>
            /// <param name="garmentImageStream">Stream de la imagen de la prenda</param>
            /// <param name="garmentFileName">Nombre del archivo de la prenda</param>
            /// <param name="garmentType">Tipo de prenda (upper, lower, dress)</param>
            /// <param name="denoiseSteps">Pasos de denoise (20-40, default: 30)</param>
            /// <param name="seed">Seed para reproducibilidad (default: 42)</param>
            /// <param name="autoCrop">Auto-crop (default: false)</param>
            /// <param name="autoMask">Auto-mask (default: true, crítico para pantalones)</param>
            /// <returns>Byte array de la imagen resultante</returns>
            public async Task<byte[]> ProcessTryOnAsync(
                Stream personImageStream,
                string personFileName,
                Stream garmentImageStream,
                string garmentFileName,
                GarmentType garmentType = GarmentType.Upper,
                int denoiseSteps = 30,
                int seed = 42,
                bool autoCrop = false,
                bool autoMask = true)
            {
                try
                {
                    using var content = new MultipartFormDataContent();

                    // Agregar imagen de persona
                    var personContent = new StreamContent(personImageStream);
                    personContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    content.Add(personContent, "person_image", personFileName);

                    // Agregar imagen de prenda
                    var garmentContent = new StreamContent(garmentImageStream);
                    garmentContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    content.Add(garmentContent, "garment_image", garmentFileName);

                    // Agregar tipo de prenda (NUEVO)
                    content.Add(new StringContent(GarmentTypeToString(garmentType)), "garment_type");

                    // Agregar parámetros
                    content.Add(new StringContent(denoiseSteps.ToString()), "denoise_steps");
                    content.Add(new StringContent(seed.ToString()), "seed");
                    content.Add(new StringContent(autoCrop.ToString().ToLower()), "auto_crop");
                    content.Add(new StringContent(autoMask.ToString().ToLower()), "auto_mask");

                    // Enviar petición
                    var response = await _httpClient.PostAsync($"{_apiBaseUrl}/tryon", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsByteArrayAsync();
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Error del servidor: {response.StatusCode} - {errorContent}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en ProcessTryOnAsync: {ex.Message}");
                    throw;
                }
            }

            /// <summary>
            /// Descarga una imagen de resultado por nombre de archivo
            /// </summary>
            public async Task<byte[]> GetResultImageAsync(string filename)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"{_apiBaseUrl}/result/{filename}");
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsByteArrayAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error obteniendo resultado: {ex.Message}");
                    throw;
                }
            }

            /// <summary>
            /// Elimina una imagen de resultado
            /// </summary>
            public async Task<bool> DeleteResultAsync(string filename)
            {
                try
                {
                    var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/result/{filename}");
                    return response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error eliminando resultado: {ex.Message}");
                    return false;
                }
            }

            /// <summary>
            /// Obtiene información sobre los tipos de prendas soportados (NUEVO)
            /// </summary>
            public Dictionary<GarmentType, GarmentTypeInfo> GetGarmentTypesInfo()
            {
                return new Dictionary<GarmentType, GarmentTypeInfo>
                {
                    {
                        GarmentType.Upper,
                        new GarmentTypeInfo
                        {
                            Name = "Parte Superior",
                            Examples = new List<string> { "Camisas", "Blusas", "T-shirts", "Suéteres", "Chaquetas" },
                            Recommendation = "Foto de medio cuerpo o torso",
                            Tip = "Funciona mejor con fotos de frente, brazos visibles"
                        }
                    },
                    {
                        GarmentType.Lower,
                        new GarmentTypeInfo
                        {
                            Name = "Parte Inferior",
                            Examples = new List<string> { "Pantalones", "Jeans", "Faldas", "Shorts" },
                            Recommendation = "⚠️ Foto de CUERPO COMPLETO requerida",
                            Tip = "Requiere foto de cuerpo completo, piernas visibles y rectas. Auto-mask debe estar activado."
                        }
                    },
                    {
                        GarmentType.Dress,
                        new GarmentTypeInfo
                        {
                            Name = "Vestido Completo",
                            Examples = new List<string> { "Vestidos", "Monos", "Prendas de cuerpo entero" },
                            Recommendation = "⚠️ Foto de CUERPO COMPLETO requerida",
                            Tip = "Requiere foto de cuerpo completo, preferiblemente de frente"
                        }
                    }
                };
            }
        }
    }
}
