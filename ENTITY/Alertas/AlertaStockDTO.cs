using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Alertas
{
    public class AlertaStockDTO
    {
        public int IdAlerta { get; set; }
        public int IdVariante { get; set; }
        public int StockActual { get; set; }
        public DateTime FechaAlerta { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? ResueltoPor { get; set; }

        // Información del producto
        public int IdArticulo { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Talla { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string CodigoSku { get; set; } = string.Empty;
        public int StockActualBd { get; set; }
        public string? ImagenProducto { get; set; }

        // Información adicional
        public int DiasPendiente { get; set; }
    }
}
