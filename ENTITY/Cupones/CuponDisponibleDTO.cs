using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Cupones
{
    public class CuponDisponibleDTO
    {
        public int IdCupon { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string TipoDescuento { get; set; } = string.Empty;
        public decimal ValorDescuento { get; set; }
        public DateTime? FechaExpiracion { get; set; }
        public char EsBienvenida { get; set; }
    }
}
