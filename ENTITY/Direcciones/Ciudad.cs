using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Direcciones
{
    public class Ciudad
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Departamento { get; set; } = null!;
        public string? CodigoDane { get; set; }

        // Relaciones
        public ICollection<Direccion> Direcciones { get; set; } = new List<Direccion>();
    }
}
