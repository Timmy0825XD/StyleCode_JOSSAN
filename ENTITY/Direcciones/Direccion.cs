using ENTITY.Pedidos;
using ENTITY.Usuarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Direcciones
{
    public class Direccion
    {
        public int Id { get; set; }
        public int CiudadId { get; set; }
        public string DireccionCompleta { get; set; } = null!;
        public string Barrio { get; set; } = null!;
        public string? CodigoPostal { get; set; }
        public string? Referencia { get; set; }

        // Relaciones
        public Ciudad Ciudad { get; set; } = null!;
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }

}
