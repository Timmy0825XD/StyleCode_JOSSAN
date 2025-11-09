using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Pedidos
{
    public class MetodoPago
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public char Estado { get; set; } = 'A';

        // Relaciones
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }

}
