using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Pedidos
{
    public class PedidoCompletoDTO
    {
        // Información del pedido
        public int IdPedido { get; set; }
        public string NumeroPedido { get; set; } = null!;
        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; } = null!;
        public decimal Subtotal { get; set; }
        public decimal CostoEnvio { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }

        // Información del cliente
        public int IdUsuario { get; set; }
        public string NombreCliente { get; set; } = null!;
        public string CorreoCliente { get; set; } = null!;
        public string TelefonoPrincipal { get; set; } = null!;

        // Información de envío
        public string DireccionCompleta { get; set; } = null!;
        public string Barrio { get; set; } = null!;
        public string Ciudad { get; set; } = null!;
        public string Departamento { get; set; } = null!;

        // Método de pago
        public string MetodoPago { get; set; } = null!;

        // Lista de productos
        public List<DetalleProductoDTO> Productos { get; set; } = new();
    }
}
