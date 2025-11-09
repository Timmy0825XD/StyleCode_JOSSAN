using DAL.Interfaces;
using Dapper;
using ENTITY.Usuarios;
using ENTITY.Utilidades;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Implementaciones
{
    public class RolDAO : IRolDAO
    {
        private readonly string _connectionString;

        public RolDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Response<RolDTO>> ObtenerTodos()
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    var query = "SELECT id_rol AS Id, nombre AS Nombre FROM roles WHERE estado = 'A'";

                    var roles = await connection.QueryAsync<RolDTO>(query);

                    return Response<RolDTO>.Done("Roles obtenidos exitosamente", list: roles.ToList());
                }
            }
            catch (Exception ex)
            {
                return Response<RolDTO>.Fail($"Error al obtener roles: {ex.Message}");
            }
        }

        public async Task<Response<RolDTO>> ObtenerPorId(int id)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    var query = "SELECT id_rol AS Id, nombre AS Nombre FROM roles WHERE id_rol = :id AND estado = 'A'";

                    var rol = await connection.QueryFirstOrDefaultAsync<RolDTO>(query, new { id });

                    if (rol != null)
                    {
                        return Response<RolDTO>.Done("Rol encontrado", rol);
                    }
                    else
                    {
                        return Response<RolDTO>.Fail("Rol no encontrado");
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<RolDTO>.Fail($"Error al obtener rol: {ex.Message}");
            }
        }
    }
}
