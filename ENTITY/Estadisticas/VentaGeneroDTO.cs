using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Estadisticas
{
    public class VentaGeneroDTO
    {
        public string Genero { get; set; } = string.Empty;
        public int CantidadPedidos { get; set; }
        public int UnidadesVendidas { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal Porcentaje { get; set; }
    }
}
