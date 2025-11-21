using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Cupones
{
    public class AplicarCuponDTO
    {
        public int IdPedido { get; set; }
        public string CodigoCupon { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
    }
}
