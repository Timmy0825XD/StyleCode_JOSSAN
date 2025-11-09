using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Articulos
{
    public class ImagenArticulo
    {
        public int Id { get; set; }
        public int ArticuloId { get; set; }
        public string Url { get; set; } = null!;
        public int Orden { get; set; } = 1;
        public char EsPrincipal { get; set; } = 'N';

        // Relaciones
        public Articulo Articulo { get; set; } = null!;
    }

}
