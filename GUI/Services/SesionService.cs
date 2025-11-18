using ENTITY.Usuarios;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace GUI.Services
{
    public class SesionService
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private LoginResponseDTO? _usuarioActual;
        private const string STORAGE_KEY = "usuario_sesion";

        public SesionService(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }
        public async Task IniciarSesion(LoginResponseDTO usuario)
        {
            _usuarioActual = usuario;
            await _sessionStorage.SetAsync(STORAGE_KEY, usuario);
        }

        public async Task CerrarSesion()
        {
            try
            {
                _usuarioActual = null;

                await _sessionStorage.DeleteAsync(STORAGE_KEY);

            }
            catch (Exception ex)
            {
                _usuarioActual = null;
            }
        }

        public async Task<LoginResponseDTO?> ObtenerUsuarioActual()
        {
            if (_usuarioActual != null)
            {
                return _usuarioActual;
            }

            try
            {
                var result = await _sessionStorage.GetAsync<LoginResponseDTO>(STORAGE_KEY);
                if (result.Success && result.Value != null)
                {
                    _usuarioActual = result.Value;
                    return _usuarioActual;
                }
            }
            catch (Exception ex)
            {
            }

            return null;
        }

        public async Task<int?> ObtenerIdUsuario()
        {
            var usuario = await ObtenerUsuarioActual();
            return usuario?.IdUsuario;
        }

        public async Task<bool> EstaLogueado()
        {
            var usuario = await ObtenerUsuarioActual();
            bool logueado = usuario != null;
            return logueado;
        }
        public async Task<bool> EsAdmin()
        {
            var usuario = await ObtenerUsuarioActual();
            return usuario?.IdRol == 1;
        }
    }
}