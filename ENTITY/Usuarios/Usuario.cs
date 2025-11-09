using ENTITY.Direcciones;
using ENTITY.Facturas;
using ENTITY.Pedidos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Usuarios
{
    public class Usuario
    {
        public int Id { get; set; }
        public int RolId { get; set; }
        public int? DireccionId { get; set; }
        public string Cedula { get; set; } = null!;
        public string PrimerNombre { get; set; } = null!;
        public string? SegundoNombre { get; set; }
        public string ApellidoPaterno { get; set; } = null!;
        public string? ApellidoMaterno { get; set; }
        public string TelefonoPrincipal { get; set; } = null!;
        public string? TelefonoSecundario { get; set; }
        public string Correo { get; set; } = null!;
        public string Contrasena { get; set; } = null!;
        public char Estado { get; set; } = 'A';
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Relaciones
        public Rol Rol { get; set; } = null!;
        public Direccion? Direccion { get; set; }
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    }
}
