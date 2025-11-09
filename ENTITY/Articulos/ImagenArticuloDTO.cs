using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Articulos
{
    public class ImagenArticuloDTO
    {
        public string UrlImagen { get; set; } = null!;
        public int Orden { get; set; } = 1;
        public char EsPrincipal { get; set; } = 'N';
    }
}
