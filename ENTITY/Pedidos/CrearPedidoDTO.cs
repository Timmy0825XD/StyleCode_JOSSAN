using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Pedidos
{
    public class CrearPedidoDTO
    {
        public int IdUsuario { get; set; }
        public int IdDireccionEnvio { get; set; }
        public int IdMetodoPago { get; set; }
        public List<ProductoPedidoDTO> Productos { get; set; } = new();
    }

}
