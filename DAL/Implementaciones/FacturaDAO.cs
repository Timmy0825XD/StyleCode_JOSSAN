using DAL.Interfaces;
using ENTITY.Facturas;
using ENTITY.Utilidades;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Implementaciones
{
    public class FacturaDAO : IFacturaDAO
    {
        private readonly string _connectionString;

        public FacturaDAO(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<Response<int>> CrearFactura(
            int idPedido,
            int idUsuario,
            string numeroFactura,
            string cufe,
            string codigoQr,
            string estadoDian,
            DateTime? fechaDian)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                using var command = new OracleCommand("PKG_FACTURACION.sp_crear_factura", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Parámetros de entrada
                command.Parameters.Add("p_id_pedido", OracleDbType.Int32).Value = idPedido;
                command.Parameters.Add("p_id_usuario", OracleDbType.Int32).Value = idUsuario;
                command.Parameters.Add("p_numero_factura", OracleDbType.Varchar2).Value = numeroFactura;
                command.Parameters.Add("p_cufe", OracleDbType.Varchar2).Value = cufe;
                command.Parameters.Add("p_codigo_qr", OracleDbType.Varchar2).Value = codigoQr;
                command.Parameters.Add("p_estado_dian", OracleDbType.Varchar2).Value = estadoDian ?? (object)DBNull.Value;
                command.Parameters.Add("p_fecha_dian", OracleDbType.Date).Value = fechaDian ?? (object)DBNull.Value;

                // Parámetro de salida
                var outputParam = new OracleParameter("p_id_factura_out", OracleDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputParam);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                int idFacturaCreada = Convert.ToInt32(outputParam.Value.ToString());

                return Response<int>.Done("Factura creada exitosamente", idFacturaCreada);
            }
            catch (OracleException ex)
            {
                return Response<int>.Fail($"Error al crear factura: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<int>.Fail($"Error inesperado: {ex.Message}");
            }
        }
        public async Task<Response<FacturaDTO>> ObtenerTodasFacturas()
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                using var command = new OracleCommand("PKG_FACTURACION.sp_obtener_todas_facturas", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(cursorParam);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                var facturas = new List<FacturaDTO>();
                while (await reader.ReadAsync())
                {
                    facturas.Add(MapearFactura(reader));
                }

                return Response<FacturaDTO>.Done("Facturas obtenidas exitosamente", list: facturas);
            }
            catch (Exception ex)
            {
                return Response<FacturaDTO>.Fail($"Error al obtener facturas: {ex.Message}");
            }
        }
        public async Task<Response<FacturaDTO>> ObtenerFacturaPorId(int idFactura)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                using var command = new OracleCommand("PKG_FACTURACION.sp_obtener_factura_por_id", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_id_factura", OracleDbType.Int32).Value = idFactura;

                var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(cursorParam);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var factura = MapearFacturaDetallada(reader);
                    return Response<FacturaDTO>.Done("Factura obtenida exitosamente", factura);
                }

                return Response<FacturaDTO>.Fail("No se encontró la factura");
            }
            catch (Exception ex)
            {
                return Response<FacturaDTO>.Fail($"Error al obtener factura: {ex.Message}");
            }
        }
        public async Task<Response<FacturaDTO>> ObtenerFacturasUsuario(int idUsuario)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                using var command = new OracleCommand("PKG_FACTURACION.sp_obtener_facturas_usuario", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_id_usuario", OracleDbType.Int32).Value = idUsuario;

                var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(cursorParam);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                var facturas = new List<FacturaDTO>();
                while (await reader.ReadAsync())
                {
                    facturas.Add(MapearFactura(reader));
                }

                return Response<FacturaDTO>.Done("Facturas del usuario obtenidas exitosamente", list: facturas);
            }
            catch (Exception ex)
            {
                return Response<FacturaDTO>.Fail($"Error al obtener facturas del usuario: {ex.Message}");
            }
        }
        public async Task<Response<FacturaDTO>> ObtenerFacturaPorNumero(string numeroFactura)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                using var command = new OracleCommand("PKG_FACTURACION.sp_obtener_factura_por_numero", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_numero_factura", OracleDbType.Varchar2).Value = numeroFactura;

                var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(cursorParam);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var factura = MapearFactura(reader);
                    return Response<FacturaDTO>.Done("Factura obtenida exitosamente", factura);
                }

                return Response<FacturaDTO>.Fail("No se encontró la factura");
            }
            catch (Exception ex)
            {
                return Response<FacturaDTO>.Fail($"Error al obtener factura: {ex.Message}");
            }
        }
        public async Task<Response<FacturaDTO>> ObtenerFacturaPorPedido(int idPedido)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                using var command = new OracleCommand("PKG_FACTURACION.sp_obtener_factura_por_pedido", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_id_pedido", OracleDbType.Int32).Value = idPedido;

                var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(cursorParam);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var factura = MapearFactura(reader);
                    return Response<FacturaDTO>.Done("Factura obtenida exitosamente", factura);
                }

                return Response<FacturaDTO>.Fail("No se encontró factura para este pedido");
            }
            catch (Exception ex)
            {
                return Response<FacturaDTO>.Fail($"Error al obtener factura: {ex.Message}");
            }
        }
        public async Task<Response<bool>> ActualizarEstado(int idFactura, string nuevoEstado)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                using var command = new OracleCommand("PKG_FACTURACION.sp_actualizar_estado", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_id_factura", OracleDbType.Int32).Value = idFactura;
                command.Parameters.Add("p_nuevo_estado", OracleDbType.Varchar2).Value = nuevoEstado;

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                return Response<bool>.Done("Estado actualizado exitosamente", true);
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error al actualizar estado: {ex.Message}");
            }
        }
        public async Task<Response<bool>> ActualizarEstadoDian(int idFactura, string estadoDian, DateTime fechaDian)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                using var command = new OracleCommand("PKG_FACTURACION.sp_actualizar_estado_dian", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_id_factura", OracleDbType.Int32).Value = idFactura;
                command.Parameters.Add("p_estado_dian", OracleDbType.Varchar2).Value = estadoDian;
                command.Parameters.Add("p_fecha_dian", OracleDbType.Date).Value = fechaDian;

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                return Response<bool>.Done("Estado DIAN actualizado exitosamente", true);
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error al actualizar estado DIAN: {ex.Message}");
            }
        }
        public async Task<Response<VerificarFacturaResult>> VerificarFacturaPedido(int idPedido)
        {
            try
            {
                    using var connection = new OracleConnection(_connectionString);
                    using var command = new OracleCommand("PKG_FACTURACION.sp_verificar_factura_pedido", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    command.Parameters.Add("p_id_pedido", OracleDbType.Int32).Value = idPedido;

                    var existeParam = new OracleParameter("p_existe", OracleDbType.Int32)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(existeParam);

                    var idFacturaParam = new OracleParameter("p_id_factura", OracleDbType.Int32)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(idFacturaParam);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    var resultado = new VerificarFacturaResult
                    {
                        Existe = Convert.ToInt32(existeParam.Value.ToString()) == 1,
                        IdFactura = Convert.ToInt32(idFacturaParam.Value.ToString())
                    };

                    return Response<VerificarFacturaResult>.Done("Verificación completada", resultado);
            }
             catch (Exception ex)
                {
                    return Response<VerificarFacturaResult>.Fail($"Error al verificar factura: {ex.Message}");
                }
            }
        private FacturaDTO MapearFactura(OracleDataReader reader)
        {
            return new FacturaDTO
            {
                Id = reader.GetInt32(reader.GetOrdinal("ID_FACTURA")),
                IdPedido = reader.GetInt32(reader.GetOrdinal("ID_PEDIDO")),
                IdUsuario = reader.GetInt32(reader.GetOrdinal("ID_USUARIO")),
                NumeroFactura = reader.GetString(reader.GetOrdinal("NUMERO_FACTURA")),
                Cufe = reader.IsDBNull(reader.GetOrdinal("CUFE")) ? null : reader.GetString(reader.GetOrdinal("CUFE")),
                CodigoQr = reader.IsDBNull(reader.GetOrdinal("CODIGO_QR")) ? null : reader.GetString(reader.GetOrdinal("CODIGO_QR")),
                FechaEmision = reader.GetDateTime(reader.GetOrdinal("FECHA_EMISION")),
                Subtotal = reader.GetDecimal(reader.GetOrdinal("SUBTOTAL")),
                Impuesto = reader.GetDecimal(reader.GetOrdinal("IMPUESTO")),
                Total = reader.GetDecimal(reader.GetOrdinal("TOTAL")),
                Estado = reader.GetString(reader.GetOrdinal("ESTADO")),
                EstadoDian = reader.IsDBNull(reader.GetOrdinal("ESTADO_DIAN")) ? null : reader.GetString(reader.GetOrdinal("ESTADO_DIAN")),
                FechaDian = reader.IsDBNull(reader.GetOrdinal("FECHA_DIAN")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_DIAN"))
            };
        }

        private FacturaDTO MapearFacturaDetallada(OracleDataReader reader)
        {
            return MapearFactura(reader);
        }
    }
}
