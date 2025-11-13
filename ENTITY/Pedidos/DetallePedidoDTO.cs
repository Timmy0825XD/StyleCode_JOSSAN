using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Pedidos
{
    public class DetallePedidoDTO
    {
        public int Id { get; set; }
        public int IdPedido { get; set; } 
        public int IdVariante { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubtotalLinea { get; set; }

        public string? NombreProducto { get; set; }
        public string? Talla { get; set; }
        public string? Color { get; set; }
    }
}
