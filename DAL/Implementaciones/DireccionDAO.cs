using DAL.Interfaces;
using ENTITY.Direcciones;
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
    public class DireccionDAO : IDireccionDAO
    {
        private readonly string _connectionString;

        public DireccionDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ========================================
        // 1. CREAR DIRECCIÓN
        // ========================================
        public async Task<Response<int>> CrearDireccion(DireccionCreacionDTO direccion)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_direcciones.crear_direccion";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_ciudad", OracleDbType.Int32).Value = direccion.CiudadId;
                        command.Parameters.Add("p_direccion_completa", OracleDbType.Varchar2).Value = direccion.DireccionCompleta;
                        command.Parameters.Add("p_barrio", OracleDbType.Varchar2).Value = direccion.Barrio;
                        command.Parameters.Add("p_codigo_postal", OracleDbType.Varchar2).Value =
                            direccion.CodigoPostal ?? (object)DBNull.Value;
                        command.Parameters.Add("p_referencia", OracleDbType.Varchar2).Value =
                            direccion.Referencia ?? (object)DBNull.Value;

                        var idParam = new OracleParameter("p_id_direccion_out", OracleDbType.Int32, ParameterDirection.Output);
                        command.Parameters.Add(idParam);

                        await command.ExecuteNonQueryAsync();

                        int idDireccionGenerado = ((OracleDecimal)idParam.Value).ToInt32();

                        return Response<int>.Done("Dirección creada exitosamente", idDireccionGenerado);
                    }
                }
            }
            catch (OracleException ex)
            {
                if (ex.Message.Contains("ORA-20001"))
                {
                    return Response<int>.Fail("La ciudad especificada no existe");
                }
                return Response<int>.Fail($"Error en Oracle: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<int>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        // ========================================
        // 2. ACTUALIZAR DIRECCIÓN
        // ========================================
        public async Task<Response<bool>> ActualizarDireccion(DireccionActualizacionDTO direccion)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_direcciones.actualizar_direccion";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_direccion", OracleDbType.Int32).Value = direccion.IdDireccion;
                        command.Parameters.Add("p_id_ciudad", OracleDbType.Int32).Value = direccion.CiudadId;
                        command.Parameters.Add("p_direccion_completa", OracleDbType.Varchar2).Value = direccion.DireccionCompleta;
                        command.Parameters.Add("p_barrio", OracleDbType.Varchar2).Value = direccion.Barrio;
                        command.Parameters.Add("p_codigo_postal", OracleDbType.Varchar2).Value =
                            direccion.CodigoPostal ?? (object)DBNull.Value;
                        command.Parameters.Add("p_referencia", OracleDbType.Varchar2).Value =
                            direccion.Referencia ?? (object)DBNull.Value;

                        await command.ExecuteNonQueryAsync();

                        return Response<bool>.Done("Dirección actualizada exitosamente", true);
                    }
                }
            }
            catch (OracleException ex)
            {
                if (ex.Message.Contains("ORA-20003"))
                {
                    return Response<bool>.Fail("La dirección no existe");
                }
                if (ex.Message.Contains("ORA-20004"))
                {
                    return Response<bool>.Fail("La ciudad especificada no existe");
                }
                return Response<bool>.Fail($"Error en Oracle: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        // ========================================
        // 3. OBTENER DIRECCIÓN POR ID
        // ========================================
        public async Task<Response<DireccionDetalleDTO>> ObtenerDireccionPorId(int idDireccion)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_direcciones.obtener_direccion_por_id";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_direccion", OracleDbType.Int32).Value = idDireccion;

                        var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                        command.Parameters.Add(cursorParam);

                        await command.ExecuteNonQueryAsync();

                        DireccionDetalleDTO direccion = null;

                        using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                        {
                            if (await reader.ReadAsync())
                            {
                                direccion = new DireccionDetalleDTO
                                {
                                    IdDireccion = reader.GetInt32(reader.GetOrdinal("ID_DIRECCION")),
                                    CiudadId = reader.GetInt32(reader.GetOrdinal("ID_CIUDAD")),
                                    DireccionCompleta = reader.GetString(reader.GetOrdinal("DIRECCION_COMPLETA")),
                                    Barrio = reader.GetString(reader.GetOrdinal("BARRIO")),
                                    CodigoPostal = reader.IsDBNull(reader.GetOrdinal("CODIGO_POSTAL"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("CODIGO_POSTAL")),
                                    Referencia = reader.IsDBNull(reader.GetOrdinal("REFERENCIA"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("REFERENCIA")),
                                    CiudadNombre = reader.GetString(reader.GetOrdinal("CIUDAD_NOMBRE")),
                                    CiudadDepartamento = reader.GetString(reader.GetOrdinal("CIUDAD_DEPARTAMENTO"))
                                };
                            }
                        }

                        if (direccion == null)
                        {
                            return Response<DireccionDetalleDTO>.Fail("Dirección no encontrada");
                        }

                        return Response<DireccionDetalleDTO>.Done("Dirección obtenida exitosamente", direccion);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<DireccionDetalleDTO>.Fail($"Error al obtener dirección: {ex.Message}");
            }
        }

        // ========================================
        // 4. OBTENER TODAS LAS CIUDADES
        // ========================================
        public async Task<Response<CiudadDTO>> ObtenerTodasLasCiudades()
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_direcciones.obtener_todas_ciudades";
                        command.CommandType = CommandType.StoredProcedure;

                        var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                        command.Parameters.Add(cursorParam);

                        await command.ExecuteNonQueryAsync();

                        var ciudades = new List<CiudadDTO>();

                        using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                var ciudad = new CiudadDTO
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ID_CIUDAD")),
                                    Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
                                    Departamento = reader.GetString(reader.GetOrdinal("DEPARTAMENTO"))
                                };

                                ciudades.Add(ciudad);
                            }
                        }

                        return Response<CiudadDTO>.Done("Ciudades obtenidas exitosamente", list: ciudades);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<CiudadDTO>.Fail($"Error al obtener ciudades: {ex.Message}");
            }
        }
    }
}
