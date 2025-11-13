using BLL.Interfaces;
using DAL.Interfaces;
using ENTITY.Facturas;
using ENTITY.Pedidos;
using ENTITY.Utilidades;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLL.Implementaciones
{
    public class FacturaService : IFacturaService
    {
        private readonly IFacturaDAO _facturaDAO;
        private readonly IPedidoDAO _pedidoDAO;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        // Configuración de Factus desde appsettings.json
        private readonly string _factusBaseUrl;
        private readonly string _factusClientId;
        private readonly string _factusClientSecret;
        private readonly string _factusUsername;
        private readonly string _factusPassword;

        public FacturaService(
            IFacturaDAO facturaDAO,
            IPedidoDAO pedidoDAO,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _facturaDAO = facturaDAO;
            _pedidoDAO = pedidoDAO;
            _httpClient = httpClient;
            _configuration = configuration;

            // Cargar configuración de Factus
            _factusBaseUrl = configuration["Factus:BaseUrl"] ?? "https://api-sandbox.factus.com.co";
            _factusClientId = configuration["Factus:ClientId"] ?? throw new Exception("Factus:ClientId no configurado");
            _factusClientSecret = configuration["Factus:ClientSecret"] ?? throw new Exception("Factus:ClientSecret no configurado");
            _factusUsername = configuration["Factus:Username"] ?? throw new Exception("Factus:Username no configurado");
            _factusPassword = configuration["Factus:Password"] ?? throw new Exception("Factus:Password no configurado");
        }
        public async Task<Response<int>> GenerarFacturaElectronica(int idPedido)
        {
            try
            {
                // Verificar si ya existe factura para este pedido
                var verificacion = await _facturaDAO.VerificarFacturaPedido(idPedido);
                if (!verificacion.IsSuccess)
                    return Response<int>.Fail(verificacion.Message);

                if (verificacion.Object.Existe)
                    return Response<int>.Fail($"Ya existe una factura para este pedido (ID Factura: {verificacion.Object.IdFactura})");

                // Obtener datos del pedido completo
                var pedidoResponse = await _pedidoDAO.ObtenerPedidoCompleto(idPedido);
                if (!pedidoResponse.IsSuccess || pedidoResponse.Object == null)
                    return Response<int>.Fail("No se pudo obtener la información del pedido");

                var pedido = pedidoResponse.Object;

                // Validar que el IdUsuario existe y es válido
                if (pedido.IdUsuario <= 0)
                {
                    return Response<int>.Fail($"El pedido no tiene un usuario válido asociado (IdUsuario: {pedido.IdUsuario})");
                }

                // Obtener token de Factus
                var tokenResponse = await ObtenerTokenFactus();
                if (!tokenResponse.IsSuccess)
                    return Response<int>.Fail($"Error al autenticar con Factus: {tokenResponse.Message}");

                var token = tokenResponse.Object;

                //  Crear request para Factus
                var factusRequest = CrearFactusRequest(pedido);

                // Enviar factura a Factus
                var factusResponse = await EnviarFacturaAFactus(factusRequest, token);
                if (!factusResponse.IsSuccess)
                    return Response<int>.Fail($"Error al crear factura en Factus: {factusResponse.Message}");

                var factusData = factusResponse.Object;

                // Guardar factura en BD
                var idFacturaCreada = await _facturaDAO.CrearFactura(
                    idPedido: pedido.IdPedido,
                    idUsuario: pedido.IdUsuario,
                    numeroFactura: factusData.Bill.Number,
                    cufe: factusData.Bill.Cufe,
                    codigoQr: factusData.Bill.Qr,
                    estadoDian: "Aprobada",
                    fechaDian: DateTime.Now
                );

                if (!idFacturaCreada.IsSuccess)
                    return Response<int>.Fail($"Error al guardar factura en BD: {idFacturaCreada.Message}");

                return Response<int>.Done(
                    $"Factura electrónica generada exitosamente. Número: {factusData.Bill.Number}",
                    idFacturaCreada.Object
                );
            }
            catch (Exception ex)
            {
                return Response<int>.Fail($"Error inesperado al generar factura: {ex.Message}");
            }
        }
        private async Task<Response<string>> ObtenerTokenFactus()
        {
            try
            {
                var tokenRequest = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "client_id", _factusClientId },
                    { "client_secret", _factusClientSecret },
                    { "username", _factusUsername },
                    { "password", _factusPassword }
                };

                var content = new FormUrlEncodedContent(tokenRequest);
                var response = await _httpClient.PostAsync($"{_factusBaseUrl}/oauth/token", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Response<string>.Fail($"Error al obtener token: {error}");
                }

                var tokenResponse = await response.Content.ReadFromJsonAsync<FactusTokenResponse>();

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                    return Response<string>.Fail("Token vacío o inválido");

                return Response<string>.Done("Token obtenido exitosamente", tokenResponse.AccessToken);
            }
            catch (Exception ex)
            {
                return Response<string>.Fail($"Error al obtener token: {ex.Message}");
            }
        }
        private FactusFacturaRequest CrearFactusRequest(PedidoCompletoDTO pedido)
        {
            var request = new FactusFacturaRequest
            {
                ReferenceCode = $"PED-{pedido.NumeroPedido}-{DateTime.Now:yyyyMMddHHmmss}",
                Observation = $"Factura del pedido {pedido.NumeroPedido}",
                PaymentMethodCode = MapearMetodoPago(pedido.MetodoPago),
                Customer = new FactusCustomer
                {
                    Identification = "222222222", // Por defecto
                    Names = pedido.NombreCliente,
                    Email = pedido.CorreoCliente,
                    Phone = pedido.TelefonoPrincipal,
                    Address = pedido.DireccionCompleta,
                    LegalOrganizationId = "2", // Persona Natural
                    TributeId = "21" // No aplica
                },
                Items = pedido.Productos?.Select(detalle => new FactusItem
                {
                    CodeReference = detalle.CodigoSKU ?? "SKU-DEFAULT",
                    Name = detalle.NombreProducto ?? "Artículo",
                    Quantity = detalle.Cantidad,
                    Price = detalle.PrecioUnitario,
                    DiscountRate = 0,
                    TaxRate = "19.00", // IVA 19%
                    UnitMeasureId = 70, // unidad
                    StandardCodeId = 1,
                    IsExcluded = 0,
                    TributeId = 1 // IVA
                }).ToList() ?? new List<FactusItem>()
            };

            return request;
        }
        private async Task<Response<FactusFacturaData>> EnviarFacturaAFactus(FactusFacturaRequest request, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var jsonContent = JsonContent.Create(request);
                var response = await _httpClient.PostAsync($"{_factusBaseUrl}/v1/bills/validate", jsonContent);

                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return Response<FactusFacturaData>.Fail($"Error HTTP {response.StatusCode}: {responseText}");

                var factusResponse = JsonSerializer.Deserialize<FactusFacturaResponse>(responseText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (factusResponse?.Data == null)
                    return Response<FactusFacturaData>.Fail("Respuesta inválida de Factus");

                return Response<FactusFacturaData>.Done("Factura creada en Factus", factusResponse.Data);
            }
            catch (Exception ex)
            {
                return Response<FactusFacturaData>.Fail($"Error al enviar factura a Factus: {ex.Message}");
            }
        }
        private string MapearMetodoPago(string? metodoPago)
        {
            return metodoPago?.ToLower() switch
            {
                "efectivo" => "10",
                "tarjeta débito" or "tarjeta debito" => "49",
                "tarjeta crédito" or "tarjeta credito" => "48",
                "transferencia" => "47",
                "consignación" or "consignacion" => "42",
                _ => "10"
            };
        }

        public async Task<Response<FacturaDTO>> ObtenerTodasFacturas()
        {
            return await _facturaDAO.ObtenerTodasFacturas();
        }

        public async Task<Response<FacturaDTO>> ObtenerFacturaPorId(int idFactura)
        {
            return await _facturaDAO.ObtenerFacturaPorId(idFactura);
        }

        public async Task<Response<FacturaDTO>> ObtenerFacturasUsuario(int idUsuario)
        {
            return await _facturaDAO.ObtenerFacturasUsuario(idUsuario);
        }

        public async Task<Response<FacturaDTO>> ObtenerFacturaPorPedido(int idPedido)
        {
            return await _facturaDAO.ObtenerFacturaPorPedido(idPedido);
        }

        public async Task<Response<bool>> ExisteFacturaParaPedido(int idPedido)
        {
            var verificacion = await _facturaDAO.VerificarFacturaPedido(idPedido);
            if (!verificacion.IsSuccess)
                return Response<bool>.Fail(verificacion.Message);

            return Response<bool>.Done(
                verificacion.Object.Existe ? "Existe factura para este pedido" : "No existe factura para este pedido",
                verificacion.Object.Existe
            );
        }
    }
}
