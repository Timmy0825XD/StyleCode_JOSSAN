using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Reportes
{
    public class ResumenCuponDTO
    {
        public string CodigoCupon { get; set; }
        public string Descripcion { get; set; }
        public string TipoDescuento { get; set; }
        public decimal ValorDescuento { get; set; }
        public int VecesUsado { get; set; }
        public decimal TotalDescontado { get; set; }
        public decimal DescuentoPromedio { get; set; }
    }
}
