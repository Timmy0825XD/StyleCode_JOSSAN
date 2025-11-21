using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Pedidos
{
    public class PedidoCompletoDTO
    {
        public int IdPedido { get; set; }
        public string NumeroPedido { get; set; } = null!;
        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; } = null!;
        public decimal Subtotal { get; set; }
        public decimal CostoEnvio { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }

        public int IdUsuario { get; set; }
        public string Cedula { get; set; } = null!;  
        public string NombreCliente { get; set; } = null!;
        public string CorreoCliente { get; set; } = null!;
        public string TelefonoPrincipal { get; set; } = null!;

        public string DireccionCompleta { get; set; } = null!;
        public string Barrio { get; set; } = null!;
        public string Ciudad { get; set; } = null!;
        public string Departamento { get; set; } = null!;

        public string MetodoPago { get; set; } = null!;

        public List<DetalleProductoDTO> Productos { get; set; } = new();
    }
}
