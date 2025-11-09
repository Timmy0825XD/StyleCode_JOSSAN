using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Articulos
{
    public class Articulo
    {
        public int Id { get; set; }
        public int CategoriaTipoId { get; set; }
        public int CategoriaOcasionId { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string Marca { get; set; } = null!;
        public string Genero { get; set; } = null!;
        public string? Material { get; set; }
        public decimal PrecioBase { get; set; }
        public char Estado { get; set; } = 'A';
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relaciones
        public CategoriaTipo CategoriaTipo { get; set; } = null!;
        public CategoriaOcasion CategoriaOcasion { get; set; } = null!;
        public ICollection<VarianteArticulo> Variantes { get; set; } = new List<VarianteArticulo>();
        public ICollection<ImagenArticulo> Imagenes { get; set; } = new List<ImagenArticulo>();
    }

}
