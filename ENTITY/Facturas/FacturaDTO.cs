using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Facturas
{
    public class FacturaDTO
    {
        public int Id { get; set; }                    // id_factura
        public int IdPedido { get; set; }              // FK hacia pedido
        public int IdUsuario { get; set; }             // FK hacia usuario
        public string NumeroFactura { get; set; } = null!;  // Ejemplo: FE-2025-00001
        public string? Cufe { get; set; }              // Código único de facturación
        public string? CodigoQr { get; set; }          // Base64 o URL del QR
        public DateTime FechaEmision { get; set; }     // Fecha de generación
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "Generada";    // Generada, Enviada, Anulada
        public string? EstadoDian { get; set; }             // Aprobada, Rechazada, Pendiente
        public DateTime? FechaDian { get; set; }            // Fecha de respuesta de la DIAN

        // Asociación con detalles de factura
        public List<DetalleFacturaDTO>? Detalles { get; set; }
    }

}
