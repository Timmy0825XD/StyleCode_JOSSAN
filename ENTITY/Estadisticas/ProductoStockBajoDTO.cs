using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Estadisticas
{
    public class ProductoStockBajoDTO
    {
        public int IdArticulo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Talla { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string CodigoSku { get; set; } = string.Empty;
        public int Stock { get; set; }
        public string? Imagen { get; set; }
    }
}
