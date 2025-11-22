using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Reportes
{
    public class ProductoMasVendidoDTO
    {
        public int IdArticulo { get; set; }
        public string NombreProducto { get; set; }
        public string Marca { get; set; }
        public string CategoriaTipo { get; set; }
        public string CategoriaOcasion { get; set; }
        public int CantidadVendida { get; set; }
        public decimal IngresosGenerados { get; set; }
        public decimal PrecioPromedio { get; set; }
    }
}
