using BLL.Interfaces;
using DAL.Interfaces;
using ENTITY.Usuarios;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Implementaciones
{
    public class RolService : IRolService
    {
        private readonly IRolDAO _rolDAO;

        public RolService(IRolDAO rolDAO)
        {
            _rolDAO = rolDAO;
        }

        public async Task<Response<RolDTO>> ObtenerTodosLosRoles()
        {
            return await _rolDAO.ObtenerTodos();
        }

        public async Task<Response<RolDTO>> ObtenerRolPorId(int id)
        {
            if (id <= 0)
            {
                return Response<RolDTO>.Fail("ID de rol inválido");
            }

            return await _rolDAO.ObtenerPorId(id);
        }
    }
}
