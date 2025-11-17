using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Usuarios
{
    public class UsuarioConDireccionDTO
    {
        public int IdUsuario { get; set; }
        public int? IdDireccion { get; set; }
        public string Cedula { get; set; } = null!;
        public string PrimerNombre { get; set; } = null!;
        public string ApellidoPaterno { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string TelefonoPrincipal { get; set; } = null!;

        // Datos de la dirección
        public string? DireccionCompleta { get; set; }
        public string? Barrio { get; set; }
        public string? CodigoPostal { get; set; }
        public string? CiudadNombre { get; set; }
        public string? Departamento { get; set; }
    }
}
