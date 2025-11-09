using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Pedidos
{
    public class ActualizarEstadoPedidoDTO
    {
        public int IdPedido { get; set; }
        public string Estado { get; set; } = null!;
    }

}
