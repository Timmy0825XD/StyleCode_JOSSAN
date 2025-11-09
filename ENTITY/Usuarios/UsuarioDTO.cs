using ENTITY.Direcciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Usuarios
{
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public int RolId { get; set; }      // Siempre 1 o 2 (ADMIN o CLIENTE)
        public int? DireccionId { get; set; }  // FK opcional (usuario recién creado puede llenarla luego)

        public string Cedula { get; set; } = null!;
        public string PrimerNombre { get; set; } = null!;
        public string? SegundoNombre { get; set; }
        public string ApellidoPaterno { get; set; } = null!;
        public string? ApellidoMaterno { get; set; }

        public string TelefonoPrincipal { get; set; } = null!;
        public string? TelefonoSecundario { get; set; }

        public string Correo { get; set; } = null!;
        public string Contrasena { get; set; } = null!;

        public string Estado { get; set; } = "A";
        public DateTime FechaRegistro { get; set; }

        // Relaciones
        public DireccionDTO? Direccion { get; set; }
        public RolDTO? Rol { get; set; }
    }

}
