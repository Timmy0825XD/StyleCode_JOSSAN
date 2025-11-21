using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Cupones
{
    public class HistorialUsoCuponDTO
    {
        public int IdUso { get; set; }
        public int IdPedido { get; set; }
        public string NumeroPedido { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string CorreoUsuario { get; set; } = string.Empty;
        public decimal DescuentoAplicado { get; set; }
        public DateTime FechaUso { get; set; }
    }
}
