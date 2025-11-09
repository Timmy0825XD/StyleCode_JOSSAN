using ENTITY.Direcciones;
using ENTITY.Facturas;
using ENTITY.Usuarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Pedidos
{
    public class Pedido
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int DireccionEnvioId { get; set; }
        public int MetodoPagoId { get; set; }
        public string NumeroPedido { get; set; } = null!;
        public DateTime FechaPedido { get; set; } = DateTime.Now;
        public string Estado { get; set; } = "Pendiente";
        public decimal Subtotal { get; set; }
        public decimal CostoEnvio { get; set; } = 0;
        public decimal Impuesto { get; set; } = 0;
        public decimal Total { get; set; }

        // Relaciones
        public Usuario Usuario { get; set; } = null!;
        public Direccion DireccionEnvio { get; set; } = null!;
        public MetodoPago MetodoPago { get; set; } = null!;
        public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
        public Factura Factura { get; set; } = null!;
    }

}
