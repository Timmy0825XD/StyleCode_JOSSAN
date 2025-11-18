using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Favoritos
{
    public class FavoritoDTO
    {
        public int IdFavorito { get; set; }
        public int IdArticulo { get; set; }
        public DateTime FechaAgregado { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public char Estado { get; set; }

        public string CategoriaTipo { get; set; } = string.Empty;
        public string CategoriaOcasion { get; set; } = string.Empty;

        public string? ImagenPrincipal { get; set; }
        public int StockTotal { get; set; }
        public int VariantesDisponibles { get; set; }
    }
}
