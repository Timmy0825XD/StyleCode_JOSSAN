using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Email
{
    public class DatosEmailPedidoDTO
    {
        public string NumeroPedido { get; set; } = string.Empty;
        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal CostoEnvio { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string DireccionCompleta { get; set; } = string.Empty;
        public string Barrio { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public List<ProductoEmailDTO> Productos { get; set; } = new();
    }
}
