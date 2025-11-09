using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Direcciones
{
    public class DireccionDTO
    {
        public int Id { get; set; }  // Para actualizar o identificar la dirección
        public int CiudadId { get; set; }  // FK hacia la ciudad seleccionada
        public string DireccionCompleta { get; set; } = null!;
        public string Barrio { get; set; } = null!;
        public string? CodigoPostal { get; set; }
        public string? Referencia { get; set; }

        // Relación con ciudad
        public CiudadDTO? Ciudad { get; set; }  // Solo para mostrar datos completos al leer
    }

}
