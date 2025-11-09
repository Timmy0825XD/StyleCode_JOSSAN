using ENTITY.Usuarios;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IUsuarioDAO
    {
        Task<Response<LoginResponseDTO>> Login(LoginRequestDTO loginRequest);
        Task<Response<int>> CrearUsuario(UsuarioDTO usuario);
        Task<Response<bool>> ActualizarUsuario(UsuarioDTO usuario);
        Task<Response<UsuarioDTO>> ObtenerUsuarios();
        Task<Response<bool>> EliminarUsuario(int idUsuario);
    }
}
