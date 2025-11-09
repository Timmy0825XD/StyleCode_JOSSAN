using ENTITY.Usuarios;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IUsuarioService
    {
        Task<Response<LoginResponseDTO>> Login(LoginRequestDTO loginRequest);
        Task<Response<int>> RegistrarUsuario(UsuarioDTO usuario);
        Task<Response<bool>> ActualizarUsuario(UsuarioDTO usuario);
        Task<Response<UsuarioDTO>> ObtenerTodosLosUsuarios();
        Task<Response<bool>> EliminarUsuario(int idUsuario);
    }
}
