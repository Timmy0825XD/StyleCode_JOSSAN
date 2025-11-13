using DAL.Interfaces;
using ENTITY.Articulos;
using ENTITY.Utilidades;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Implementaciones
{
    public class CategoriaDAO : ICategoriaDAO
    {
        private readonly string _connectionString;

        public CategoriaDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Response<CategoriaTipoDTO>> ObtenerCategoriasTipo()
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_articulos.listar_categorias_tipo";
                        command.CommandType = CommandType.StoredProcedure;

                        var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                        command.Parameters.Add(cursorParam);

                        await command.ExecuteNonQueryAsync();

                        var categorias = new List<CategoriaTipoDTO>();

                        using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                var categoria = new CategoriaTipoDTO
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ID_CATEGORIA_TIPO")),
                                    Nombre = reader.GetString(reader.GetOrdinal("NOMBRE"))
                                };

                                categorias.Add(categoria);
                            }
                        }

                        return Response<CategoriaTipoDTO>.Done("Categorías tipo obtenidas exitosamente", list: categorias);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<CategoriaTipoDTO>.Fail($"Error al obtener categorías tipo: {ex.Message}");
            }
        }

        public async Task<Response<CategoriaOcasionDTO>> ObtenerCategoriasOcasion()
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_articulos.listar_categorias_ocasion";
                        command.CommandType = CommandType.StoredProcedure;

                        var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                        command.Parameters.Add(cursorParam);

                        await command.ExecuteNonQueryAsync();

                        var categorias = new List<CategoriaOcasionDTO>();

                        using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                var categoria = new CategoriaOcasionDTO
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ID_CATEGORIA_OCASION")),
                                    Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
                                    Descripcion = reader.IsDBNull(reader.GetOrdinal("DESCRIPCION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPCION"))
                                };

                                categorias.Add(categoria);
                            }
                        }

                        return Response<CategoriaOcasionDTO>.Done("Categorías ocasión obtenidas exitosamente", list: categorias);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<CategoriaOcasionDTO>.Fail($"Error al obtener categorías ocasión: {ex.Message}");
            }
        }
    }
}
