using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Direcciones
{
    public class CiudadDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Departamento { get; set; } = null!;
    }
}
