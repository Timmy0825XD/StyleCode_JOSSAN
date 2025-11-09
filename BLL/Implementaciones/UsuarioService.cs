using BLL.Interfaces;
using DAL.Interfaces;
using ENTITY.Usuarios;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BLL.Implementaciones
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioDAO _usuarioDAO;

        public UsuarioService(IUsuarioDAO usuarioDAO)
        {
            _usuarioDAO = usuarioDAO;
        }

        public async Task<Response<LoginResponseDTO>> Login(LoginRequestDTO loginRequest)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(loginRequest.Correo))
            {
                return Response<LoginResponseDTO>.Fail("El correo es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(loginRequest.Contrasena))
            {
                return Response<LoginResponseDTO>.Fail("La contraseña es obligatoria");
            }

            return await _usuarioDAO.Login(loginRequest);
        }

        public async Task<Response<int>> RegistrarUsuario(UsuarioDTO usuario)
        {
            // Validaciones de negocio
            var validacion = ValidarDatosUsuario(usuario);
            if (!validacion.IsSuccess)
            {
                return Response<int>.Fail(validacion.Message);
            }

            // Establecer rol por defecto a Cliente (2) si no viene especificado
            if (usuario.RolId == 0)
            {
                usuario.RolId = 2;
            }

            // Aquí podrías hashear la contraseña
            // usuario.Contrasena = HashPassword(usuario.Contrasena);

            return await _usuarioDAO.CrearUsuario(usuario);
        }

        public async Task<Response<bool>> ActualizarUsuario(UsuarioDTO usuario)
        {
            // Validaciones
            if (usuario.Id <= 0)
            {
                return Response<bool>.Fail("ID de usuario inválido");
            }

            var validacion = ValidarDatosUsuario(usuario);
            if (!validacion.IsSuccess)
            {
                return Response<bool>.Fail(validacion.Message);
            }

            return await _usuarioDAO.ActualizarUsuario(usuario);
        }

        public async Task<Response<UsuarioDTO>> ObtenerTodosLosUsuarios()
        {
            return await _usuarioDAO.ObtenerUsuarios();
        }

        public async Task<Response<bool>> EliminarUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return Response<bool>.Fail("ID de usuario inválido");
            }

            return await _usuarioDAO.EliminarUsuario(idUsuario);
        }

        // Método privado de validación
        private Response<bool> ValidarDatosUsuario(UsuarioDTO usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.Cedula) || usuario.Cedula.Length < 8)
            {
                return Response<bool>.Fail("La cédula debe tener al menos 8 caracteres");
            }

            if (string.IsNullOrWhiteSpace(usuario.PrimerNombre))
            {
                return Response<bool>.Fail("El primer nombre es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(usuario.ApellidoPaterno))
            {
                return Response<bool>.Fail("El apellido paterno es obligatorio");
            }

            if (!EsCorreoValido(usuario.Correo))
            {
                return Response<bool>.Fail("El formato del correo es inválido");
            }

            if (string.IsNullOrWhiteSpace(usuario.Contrasena) || usuario.Contrasena.Length < 6)
            {
                return Response<bool>.Fail("La contraseña debe tener al menos 6 caracteres");
            }

            if (string.IsNullOrWhiteSpace(usuario.TelefonoPrincipal))
            {
                return Response<bool>.Fail("El teléfono principal es obligatorio");
            }

            return Response<bool>.Done("Validación exitosa");
        }

        private bool EsCorreoValido(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(correo);
            }
            catch
            {
                return false;
            }
        }
    }
}
