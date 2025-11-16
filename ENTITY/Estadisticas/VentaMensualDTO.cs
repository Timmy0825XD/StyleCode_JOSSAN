using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Estadisticas
{
    public class VentaMensualDTO
    {
        public string MesNombre { get; set; } = string.Empty;
        public int MesNumero { get; set; }
        public decimal TotalVentas { get; set; }
        public int CantidadPedidos { get; set; }
    }
}
