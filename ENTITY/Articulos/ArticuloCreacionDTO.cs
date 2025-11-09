using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Articulos
{
    public class ArticuloCreacionDTO
    {
        public int CategoriaTipoId { get; set; }
        public int CategoriaOcasionId { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string Marca { get; set; } = null!;
        public string Genero { get; set; } = null!; // Hombre, Mujer, Unisex, etc.
        public string? Material { get; set; }
        public decimal PrecioBase { get; set; }
        public char Estado { get; set; } = 'A';  // siempre 'A'

        public List<VarianteArticuloDTO> Variantes { get; set; } = new();
        public List<ImagenArticuloDTO> Imagenes { get; set; } = new();
    }
}
