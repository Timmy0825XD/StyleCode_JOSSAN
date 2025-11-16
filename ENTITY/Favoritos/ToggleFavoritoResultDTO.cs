using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Favoritos
{
    public class ToggleFavoritoResultDTO
    {
        public int Agregado { get; set; } // 1 = agregado, 0 = eliminado
        public string Mensaje { get; set; } = string.Empty;
    }
}
