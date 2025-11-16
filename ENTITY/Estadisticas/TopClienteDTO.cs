using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Estadisticas
{
    public class TopClienteDTO
    {
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string TelefonoPrincipal { get; set; } = string.Empty;
        public int TotalPedidos { get; set; }
        public decimal TotalGastado { get; set; }
        public decimal TicketPromedio { get; set; }
        public DateTime UltimaCompra { get; set; }
    }
}
