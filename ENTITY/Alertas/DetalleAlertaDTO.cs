using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Alertas
{
    public class DetalleAlertaDTO
    {
        public int IdAlerta { get; set; }
        public int StockAlCrearAlerta { get; set; }
        public DateTime FechaAlerta { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? ResueltoPor { get; set; }

        // Información del artículo
        public int IdArticulo { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }

        // Información de la variante
        public int IdVariante { get; set; }
        public string Talla { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string CodigoSku { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public string? ImagenProducto { get; set; }

        // Tiempo de resolución
        public int DiasResolucion { get; set; }
    }
}
