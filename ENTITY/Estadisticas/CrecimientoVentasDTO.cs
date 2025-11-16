using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Estadisticas
{
    public class CrecimientoVentasDTO
    {
        // MES ACTUAL VS MES ANTERIOR
        public decimal VentasMesActual { get; set; }
        public decimal VentasMesAnterior { get; set; }
        public decimal CrecimientoPorcentajeMes { get; set; }

        // AÑO ACTUAL VS AÑO ANTERIOR
        public decimal VentasAnioActual { get; set; }
        public decimal VentasAnioAnterior { get; set; }
        public decimal CrecimientoPorcentajeAnio { get; set; }
    }
}
