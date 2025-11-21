using DAL.Interfaces;
using ENTITY.Cupones;
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
    public class CuponDAO : ICuponDAO
    {
        private readonly string _connectionString;

        public CuponDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Response<int>> CrearCupon(CrearCuponDTO cupon, bool enviarEmailMasivo = false)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_cupones.sp_crear_cupon";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_codigo", OracleDbType.Varchar2).Value = cupon.Codigo;
                        command.Parameters.Add("p_descripcion", OracleDbType.Varchar2).Value = cupon.Descripcion;
                        command.Parameters.Add("p_tipo_descuento", OracleDbType.Varchar2).Value = cupon.TipoDescuento;
                        command.Parameters.Add("p_valor_descuento", OracleDbType.Decimal).Value = cupon.ValorDescuento;
                        command.Parameters.Add("p_usos_maximos", OracleDbType.Int32).Value =
                            cupon.UsosMaximos.HasValue ? (object)cupon.UsosMaximos.Value : DBNull.Value;
                        command.Parameters.Add("p_fecha_expiracion", OracleDbType.Date).Value =
                            cupon.FechaExpiracion.HasValue ? (object)cupon.FechaExpiracion.Value : DBNull.Value;

                        var idParam = new OracleParameter("p_id_cupon_out", OracleDbType.Int32, ParameterDirection.Output);
                        command.Parameters.Add(idParam);

                        await command.ExecuteNonQueryAsync();

                        int idCuponCreado = ((OracleDecimal)idParam.Value).ToInt32();

                        // Enviar email masivo si se solicita
                        if (enviarEmailMasivo)
                        {
                            await EnviarEmailMasivoCupon(connection, idCuponCreado);
                        }

                        return Response<int>.Done("Cupón creado exitosamente", idCuponCreado);
                    }
                }
            }
            catch (OracleException ex)
            {
                if (ex.Message.Contains("ORA-20001"))
                    return Response<int>.Fail("El código del cupón ya existe");
                if (ex.Message.Contains("ORA-20002"))
                    return Response<int>.Fail("Tipo de descuento inválido");
                if (ex.Message.Contains("ORA-20003"))
                    return Response<int>.Fail("El porcentaje debe estar entre 1 y 100");
                if (ex.Message.Contains("ORA-20004"))
                    return Response<int>.Fail("El valor del descuento debe ser mayor a 0");

                return Response<int>.Fail($"Error en Oracle: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<int>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<Response<CuponDTO>> ObtenerTodosCupones()
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_cupones.sp_obtener_todos_cupones";
                        command.CommandType = CommandType.StoredProcedure;

                        var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                        command.Parameters.Add(cursorParam);

                        await command.ExecuteNonQueryAsync();

                        var cupones = new List<CuponDTO>();

                        using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                cupones.Add(new CuponDTO
                                {
                                    IdCupon = reader.GetInt32(reader.GetOrdinal("ID_CUPON")),
                                    Codigo = reader.GetString(reader.GetOrdinal("CODIGO")),
                                    Descripcion = reader.GetString(reader.GetOrdinal("DESCRIPCION")),
                                    TipoDescuento = reader.GetString(reader.GetOrdinal("TIPO_DESCUENTO")),
                                    ValorDescuento = reader.GetDecimal(reader.GetOrdinal("VALOR_DESCUENTO")),
                                    UsosMaximos = reader.IsDBNull(reader.GetOrdinal("USOS_MAXIMOS")) ? null : reader.GetInt32(reader.GetOrdinal("USOS_MAXIMOS")),
                                    UsosActuales = reader.GetInt32(reader.GetOrdinal("USOS_ACTUALES")),
                                    FechaInicio = reader.GetDateTime(reader.GetOrdinal("FECHA_INICIO")),
                                    FechaExpiracion = reader.IsDBNull(reader.GetOrdinal("FECHA_EXPIRACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_EXPIRACION")),
                                    Estado = reader.GetString(reader.GetOrdinal("ESTADO"))[0],
                                    EsBienvenida = reader.GetString(reader.GetOrdinal("ES_BIENVENIDA"))[0],
                                    FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION"))
                                });
                            }
                        }

                        return Response<CuponDTO>.Done("Cupones obtenidos exitosamente", default, cupones);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<CuponDTO>.Fail($"Error al obtener cupones: {ex.Message}");
            }
        }

        public async Task<Response<bool>> DesactivarCupon(int idCupon)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_cupones.sp_desactivar_cupon";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_cupon", OracleDbType.Int32).Value = idCupon;

                        await command.ExecuteNonQueryAsync();

                        return Response<bool>.Done("Cupón desactivado exitosamente", true);
                    }
                }
            }
            catch (OracleException ex)
            {
                if (ex.Message.Contains("ORA-20020"))
                    return Response<bool>.Fail("Cupón no encontrado");

                return Response<bool>.Fail($"Error en Oracle: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<Response<HistorialUsoCuponDTO>> ObtenerHistorialUso(int idCupon)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_cupones.sp_obtener_historial_uso";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_cupon", OracleDbType.Int32).Value = idCupon;
                        var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                        command.Parameters.Add(cursorParam);

                        await command.ExecuteNonQueryAsync();

                        var historial = new List<HistorialUsoCuponDTO>();

                        using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                historial.Add(new HistorialUsoCuponDTO
                                {
                                    IdUso = reader.GetInt32(reader.GetOrdinal("ID_USO")),
                                    IdPedido = reader.GetInt32(reader.GetOrdinal("ID_PEDIDO")),
                                    NumeroPedido = reader.GetString(reader.GetOrdinal("NUMERO_PEDIDO")),
                                    NombreUsuario = reader.GetString(reader.GetOrdinal("NOMBRE_USUARIO")),
                                    CorreoUsuario = reader.GetString(reader.GetOrdinal("CORREO_USUARIO")),
                                    DescuentoAplicado = reader.GetDecimal(reader.GetOrdinal("DESCUENTO_APLICADO")),
                                    FechaUso = reader.GetDateTime(reader.GetOrdinal("FECHA_USO"))
                                });
                            }
                        }

                        return Response<HistorialUsoCuponDTO>.Done("Historial obtenido exitosamente", default, historial);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<HistorialUsoCuponDTO>.Fail($"Error al obtener historial: {ex.Message}");
            }
        }

        public async Task<Response<ResultadoValidacionDTO>> ValidarCupon(ValidarCuponDTO validacion)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_cupones.sp_validar_cupon";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_codigo", OracleDbType.Varchar2).Value = validacion.Codigo;
                        command.Parameters.Add("p_id_usuario", OracleDbType.Int32).Value = validacion.IdUsuario;
                        command.Parameters.Add("p_subtotal", OracleDbType.Decimal).Value = validacion.Subtotal;

                        var esValidoParam = new OracleParameter("p_es_valido", OracleDbType.Int32, ParameterDirection.Output);
                        var mensajeParam = new OracleParameter("p_mensaje", OracleDbType.Varchar2, 200, null, ParameterDirection.Output);
                        var descuentoParam = new OracleParameter("p_descuento", OracleDbType.Decimal, ParameterDirection.Output);
                        var idCuponParam = new OracleParameter("p_id_cupon", OracleDbType.Int32, ParameterDirection.Output);

                        command.Parameters.Add(esValidoParam);
                        command.Parameters.Add(mensajeParam);
                        command.Parameters.Add(descuentoParam);
                        command.Parameters.Add(idCuponParam);

                        await command.ExecuteNonQueryAsync();

                        var resultado = new ResultadoValidacionDTO
                        {
                            EsValido = ((OracleDecimal)esValidoParam.Value).ToInt32() == 1,
                            Mensaje = mensajeParam.Value.ToString() ?? "Error desconocido",
                            Descuento = descuentoParam.Value != DBNull.Value ? ((OracleDecimal)descuentoParam.Value).Value : 0,
                            IdCupon = idCuponParam.Value != DBNull.Value ? ((OracleDecimal)idCuponParam.Value).ToInt32() : 0
                        };

                        return Response<ResultadoValidacionDTO>.Done(resultado.Mensaje, resultado);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<ResultadoValidacionDTO>.Fail($"Error al validar cupón: {ex.Message}");
            }
        }

        public async Task<Response<CuponDisponibleDTO>> ObtenerCuponesDisponibles(int idUsuario)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_cupones.sp_obtener_cupones_disponibles";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_usuario", OracleDbType.Int32).Value = idUsuario;
                        var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                        command.Parameters.Add(cursorParam);

                        await command.ExecuteNonQueryAsync();

                        var cupones = new List<CuponDisponibleDTO>();

                        using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                cupones.Add(new CuponDisponibleDTO
                                {
                                    IdCupon = reader.GetInt32(reader.GetOrdinal("ID_CUPON")),
                                    Codigo = reader.GetString(reader.GetOrdinal("CODIGO")),
                                    Descripcion = reader.GetString(reader.GetOrdinal("DESCRIPCION")),
                                    TipoDescuento = reader.GetString(reader.GetOrdinal("TIPO_DESCUENTO")),
                                    ValorDescuento = reader.GetDecimal(reader.GetOrdinal("VALOR_DESCUENTO")),
                                    FechaExpiracion = reader.IsDBNull(reader.GetOrdinal("FECHA_EXPIRACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_EXPIRACION")),
                                    EsBienvenida = reader.GetString(reader.GetOrdinal("ES_BIENVENIDA"))[0]
                                });
                            }
                        }

                        return Response<CuponDisponibleDTO>.Done("Cupones disponibles obtenidos", default, cupones);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<CuponDisponibleDTO>.Fail($"Error al obtener cupones disponibles: {ex.Message}");
            }
        }

        public async Task<Response<bool>> AplicarCupon(AplicarCuponDTO aplicacion)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_cupones.sp_aplicar_cupon";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_pedido", OracleDbType.Int32).Value = aplicacion.IdPedido;
                        command.Parameters.Add("p_codigo_cupon", OracleDbType.Varchar2).Value = aplicacion.CodigoCupon;
                        command.Parameters.Add("p_id_usuario", OracleDbType.Int32).Value = aplicacion.IdUsuario;

                        await command.ExecuteNonQueryAsync();

                        return Response<bool>.Done("Cupón aplicado exitosamente", true);
                    }
                }
            }
            catch (OracleException ex)
            {
                if (ex.Message.Contains("ORA-20010"))
                {
                    // Extraer mensaje específico
                    var mensaje = ex.Message.Substring(ex.Message.IndexOf("ORA-20010") + 12);
                    return Response<bool>.Fail(mensaje);
                }

                return Response<bool>.Fail($"Error en Oracle: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        private async Task EnviarEmailMasivoCupon(OracleConnection connection, int idCupon)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "pkg_emails.enviar_email_cupon_masivo";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("p_id_cupon", OracleDbType.Int32).Value = idCupon;

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
