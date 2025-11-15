using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Email
{
    public class ColaEmailDTO
    {
        public int IdEmail { get; set; }
        public string TipoEmail { get; set; } = string.Empty;
        public int IdPedido { get; set; }
        public string Destinatario { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public int Intentos { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
