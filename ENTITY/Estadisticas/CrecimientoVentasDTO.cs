using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Estadisticas
{
    public class CrecimientoVentasDTO
    {
        public decimal VentasMesActual { get; set; }
        public decimal VentasMesAnterior { get; set; }
        public decimal CrecimientoPorcentajeMes { get; set; }

        public decimal VentasAnioActual { get; set; }
        public decimal VentasAnioAnterior { get; set; }
        public decimal CrecimientoPorcentajeAnio { get; set; }
    }
}
