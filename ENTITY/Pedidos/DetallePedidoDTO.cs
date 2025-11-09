using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Pedidos
{
    public class DetallePedidoDTO
    {
        public int Id { get; set; }                    // id_detalle
        public int IdPedido { get; set; }              // FK al pedido
        public int IdVariante { get; set; }            // Variante del producto
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubtotalLinea { get; set; }

        // Opcional: mostrar info del producto
        public string? NombreProducto { get; set; }
        public string? Talla { get; set; }
        public string? Color { get; set; }
    }
}
