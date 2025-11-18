using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Pedidos
{
    public class CarritoItemDTO
    {
        public int IdVariante { get; set; }
        public int IdArticulo { get; set; }
        public string NombreArticulo { get; set; } = null!;
        public string Marca { get; set; } = null!;
        public string Talla { get; set; } = null!;
        public string Color { get; set; } = null!;
        public string CodigoSKU { get; set; } = null!;
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public int StockDisponible { get; set; }
        public string? ImagenUrl { get; set; }

        public decimal Subtotal => PrecioUnitario * Cantidad;
    }
}
