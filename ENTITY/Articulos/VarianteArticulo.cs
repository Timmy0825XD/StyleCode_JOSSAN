using ENTITY.Facturas;
using ENTITY.Pedidos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Articulos
{
    public class VarianteArticulo
    {
        public int Id { get; set; }
        public int ArticuloId { get; set; }
        public string Talla { get; set; } = null!;
        public string Color { get; set; } = null!;
        public string CodigoSKU { get; set; } = null!;
        public int Stock { get; set; } = 0;
        public char Estado { get; set; } = 'A';

        // Relaciones
        public Articulo Articulo { get; set; } = null!;
        public ICollection<DetallePedido> DetallesPedidos { get; set; } = new List<DetallePedido>();
        public ICollection<DetalleFactura> DetallesFactura { get; set; } = new List<DetalleFactura>();
    }

}
