using ENTITY.Pedidos;
using ENTITY.Usuarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Facturas
{
    public class Factura
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int UsuarioId { get; set; }
        public string NumeroFactura { get; set; } = null!;
        public string? Cufe { get; set; }
        public string? CodigoQR { get; set; }
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; } = 0;
        public decimal Total { get; set; }
        public string Estado { get; set; } = "Generada";
        public string? EstadoDian { get; set; }
        public DateTime? FechaDian { get; set; }

        // Relaciones
        public Pedido Pedido { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
    }
}
