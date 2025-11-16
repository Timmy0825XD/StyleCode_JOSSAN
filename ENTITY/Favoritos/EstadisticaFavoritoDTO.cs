using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Favoritos
{
    public class EstadisticaFavoritoDTO
    {
        public int IdArticulo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public int VecesFavorito { get; set; }
        public string? ImagenPrincipal { get; set; }
        public DateTime UltimoFavorito { get; set; }
    }
}
