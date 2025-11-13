using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Facturas
{
    public class FacturaDTO
    {
        public int Id { get; set; }
        public int IdPedido { get; set; }
        public int IdUsuario { get; set; }
        public string NumeroFactura { get; set; } = null!;
        public string? Cufe { get; set; }  
        public string? CodigoQr { get; set; }    
        public DateTime FechaEmision { get; set; } 
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "Generada"; 
        public string? EstadoDian { get; set; }   
        public DateTime? FechaDian { get; set; }

        public List<DetalleFacturaDTO>? Detalles { get; set; }
    }

}
