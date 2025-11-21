using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Cupones
{
    public class CrearCuponDTO
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string TipoDescuento { get; set; } = "PORCENTAJE"; // PORCENTAJE o MONTO
        public decimal ValorDescuento { get; set; }
        public int? UsosMaximos { get; set; } // null = ilimitado
        public DateTime? FechaExpiracion { get; set; } // null = sin expiración
    }
}
