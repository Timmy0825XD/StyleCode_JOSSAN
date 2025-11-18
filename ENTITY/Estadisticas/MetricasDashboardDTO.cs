using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Estadisticas
{
    public class MetricasDashboardDTO
    {
        public decimal VentasMesActual { get; set; }
        public decimal VentasMesAnterior { get; set; }
        public decimal VentasHoy { get; set; }

        public int TotalPedidosMes { get; set; }
        public int PedidosPendientes { get; set; }
        public int PedidosHoy { get; set; }

        public int TotalClientes { get; set; }
        public int ClientesNuevosMes { get; set; }

        public int TotalProductosActivos { get; set; }
        public int ProductosStockBajo { get; set; }

        public decimal TicketPromedio { get; set; }
        public decimal IvaMes { get; set; }
        public decimal TasaEntregaPorcentaje { get; set; }
    }
}
