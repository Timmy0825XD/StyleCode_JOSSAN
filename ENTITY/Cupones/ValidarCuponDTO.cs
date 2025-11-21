using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Cupones
{
    public class ValidarCuponDTO
    {
        public string Codigo { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
