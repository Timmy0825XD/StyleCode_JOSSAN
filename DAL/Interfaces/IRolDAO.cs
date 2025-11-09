using ENTITY.Usuarios;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IRolDAO
    {
        Task<Response<RolDTO>> ObtenerTodos();
        Task<Response<RolDTO>> ObtenerPorId(int id);
    }
}
