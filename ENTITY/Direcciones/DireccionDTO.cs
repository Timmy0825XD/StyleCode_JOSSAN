using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Direcciones
{
    public class DireccionDTO
    {
        public int Id { get; set; }
        public int CiudadId { get; set; }
        public string DireccionCompleta { get; set; } = null!;
        public string Barrio { get; set; } = null!;
        public string? CodigoPostal { get; set; }
        public string? Referencia { get; set; }

        public CiudadDTO? Ciudad { get; set; }
    }

}
