using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Facturas
{
    public class DetalleFacturaDTO
    {
        public int Id { get; set; }
        public int IdFactura { get; set; } 
        public int IdVariante { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubtotalLinea { get; set; }
    }

}
