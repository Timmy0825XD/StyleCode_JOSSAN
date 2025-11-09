using ENTITY.Direcciones;
using ENTITY.Usuarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Pedidos
{
    public class PedidoDTO
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public int IdDireccionEnvio { get; set; }
        public int IdMetodoPago { get; set; }

        public string NumeroPedido { get; set; } = null!;
        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; } = "Pendiente";

        public decimal Subtotal { get; set; }
        public decimal CostoEnvio { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }

        // Relaciones
        public UsuarioDTO? Usuario { get; set; }
        public DireccionDTO? DireccionEnvio { get; set; }
        public MetodoPagoDTO? MetodoPago { get; set; }

        // Lista de detalles
        public List<DetallePedidoDTO> Detalles { get; set; } = new();
    }

}
