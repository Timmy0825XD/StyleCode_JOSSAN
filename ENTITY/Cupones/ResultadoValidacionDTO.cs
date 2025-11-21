using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Cupones
{
    public class ResultadoValidacionDTO
    {
        public bool EsValido { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public decimal Descuento { get; set; }
        public int IdCupon { get; set; }
    }
}
