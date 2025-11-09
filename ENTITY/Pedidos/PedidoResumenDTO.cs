using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Pedidos
{
    public class PedidoResumenDTO
    {
        public int Id { get; set; }
        public string NumeroPedido { get; set; } = null!;
        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; } = null!;
        public decimal Total { get; set; }
    }

}
