using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Reportes
{
    public class ResumenFinancieroDTO
    {
        public decimal TotalVentas { get; set; }
        public int TotalPedidos { get; set; }
        public decimal TicketPromedio { get; set; }
        public decimal TotalDescuentos { get; set; }
        public decimal IngresosNetos { get; set; }
        public decimal IvaTotal { get; set; }
        public decimal SubtotalTotal { get; set; }
        public int ClientesActivos { get; set; }
        public int ClientesNuevos { get; set; }
    }
}
