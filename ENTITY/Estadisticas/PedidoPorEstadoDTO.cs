using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Estadisticas
{
    public class PedidoPorEstadoDTO
    {
        public string Estado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal Porcentaje { get; set; }
    }
}
