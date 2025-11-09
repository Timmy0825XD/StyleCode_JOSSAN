using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Facturas
{
    public class DetalleFacturaDTO
    {
        public int Id { get; set; }                   // id_detalle
        public int IdFactura { get; set; }            // FK hacia factura
        public int IdVariante { get; set; }           // FK hacia variante de artículo
        public int Cantidad { get; set; }             // Unidades facturadas
        public decimal PrecioUnitario { get; set; }   // Precio por unidad
        public decimal SubtotalLinea { get; set; }    // Total línea = cantidad * precio
    }

}
