using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Estadisticas
{
    public class PedidoRecienteDTO
    {
        public int IdPedido { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string EmailCliente { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public int CantidadProductos { get; set; }
    }
}
