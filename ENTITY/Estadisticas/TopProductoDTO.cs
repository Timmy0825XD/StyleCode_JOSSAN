using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Estadisticas
{
    public class TopProductoDTO
    {
        public int IdArticulo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public int UnidadesVendidas { get; set; }
        public int CantidadPedidos { get; set; }
        public decimal IngresosGenerados { get; set; }
        public string? Imagen { get; set; }
    }
}
