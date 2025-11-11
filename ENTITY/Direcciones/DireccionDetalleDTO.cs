using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Direcciones
{
    public class DireccionDetalleDTO
    {
        public int IdDireccion { get; set; }
        public int CiudadId { get; set; }
        public string DireccionCompleta { get; set; } = null!;
        public string Barrio { get; set; } = null!;
        public string? CodigoPostal { get; set; }
        public string? Referencia { get; set; }

        // Datos de la ciudad
        public string CiudadNombre { get; set; } = null!;
        public string CiudadDepartamento { get; set; } = null!;
    }
}
