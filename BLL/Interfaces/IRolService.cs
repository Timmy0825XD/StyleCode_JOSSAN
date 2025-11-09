using ENTITY.Usuarios;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IRolService
    {
        Task<Response<RolDTO>> ObtenerTodosLosRoles();
        Task<Response<RolDTO>> ObtenerRolPorId(int id);
    }
}
