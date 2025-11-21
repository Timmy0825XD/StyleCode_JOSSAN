using DAL.Interfaces;
using ENTITY.Pedidos;
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
    public class PedidoDAO : IPedidoDAO
    {
        private readonly string _connectionString;

        public PedidoDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Response<int>> CrearPedido(CrearPedidoDTO pedido)
        {
            OracleConnection connection = null;
            OracleTransaction transaction = null;

            try
            {
                connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                transaction = connection.BeginTransaction();

                int idPedidoGenerado = 0;

                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "pkg_pedidos.sp_crear_pedido";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("p_id_usuario", OracleDbType.Int32).Value = pedido.IdUsuario;
                    command.Parameters.Add("p_id_direccion_envio", OracleDbType.Int32).Value = pedido.IdDireccionEnvio;
                    command.Parameters.Add("p_id_metodo_pago", OracleDbType.Int32).Value = pedido.IdMetodoPago;

                    var idParam = new OracleParameter("p_id_pedido_generado", OracleDbType.Int32, ParameterDirection.Output);
                    command.Parameters.Add(idParam);

                    await command.ExecuteNonQueryAsync();

                    idPedidoGenerado = ((OracleDecimal)idParam.Value).ToInt32();
                }

                foreach (var producto in pedido.Productos)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = "pkg_pedidos.sp_agregar_detalle_pedido";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_pedido", OracleDbType.Int32).Value = idPedidoGenerado;
                        command.Parameters.Add("p_id_variante", OracleDbType.Int32).Value = producto.IdVariante;
                        command.Parameters.Add("p_cantidad", OracleDbType.Int32).Value = producto.Cantidad;

                        await command.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();

                return Response<int>.Done("Pedido creado exitosamente", idPedidoGenerado);
            }
            catch (OracleException ex)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();

                if (ex.Message.Contains("ORA-20001"))
                {
                    return Response<int>.Fail("Usuario no encontrado o inactivo");
                }
                if (ex.Message.Contains("ORA-20002"))
                {
                    return Response<int>.Fail("Dirección de envío no encontrada");
                }
                if (ex.Message.Contains("ORA-20003"))
                {
                    return Response<int>.Fail("Método de pago no encontrado o inactivo");
                }
                if (ex.Message.Contains("ORA-20013"))
                {
                    var mensaje = ex.Message.Substring(ex.Message.IndexOf("ORA-20013") + 12);
                    return Response<int>.Fail($"Stock insuficiente: {mensaje}");
                }
                if (ex.Message.Contains("ORA-20014"))
                {
                    return Response<int>.Fail("Una o más variantes de producto no fueron encontradas");
                }

                return Response<int>.Fail($"Error en Oracle: {ex.Message}");
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();

                return Response<int>.Fail($"Error inesperado: {ex.Message}");
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    await connection.CloseAsync();
                    await connection.DisposeAsync();
                }
            }
        }

        public async Task<Response<PedidoDTO>> ObtenerPedidosUsuario(int idUsuario)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_pedidos.sp_obtener_pedidos_usuario";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_usuario", OracleDbType.Int32).Value = idUsuario;

                        var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                        command.Parameters.Add(cursorParam);

                        await command.ExecuteNonQueryAsync();

                        var pedidos = new List<PedidoDTO>();

                        using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                pedidos.Add(new PedidoDTO
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ID_PEDIDO")),
                                    NumeroPedido = reader.GetString(reader.GetOrdinal("NUMERO_PEDIDO")),
                                    FechaPedido = reader.GetDateTime(reader.GetOrdinal("FECHA_PEDIDO")),
                                    Estado = reader.GetString(reader.GetOrdinal("ESTADO")),
                                    Subtotal = reader.GetDecimal(reader.GetOrdinal("SUBTOTAL")),
                                    CostoEnvio = reader.GetDecimal(reader.GetOrdinal("COSTO_ENVIO")),
                                    Impuesto = reader.GetDecimal(reader.GetOrdinal("IMPUESTO")),
                                    Total = reader.GetDecimal(reader.GetOrdinal("TOTAL")),
                                    MetodoPago = new MetodoPagoDTO
                                    {
                                        Nombre = reader.GetString(reader.GetOrdinal("METODO_PAGO"))
                                    },
                                    DireccionEnvio = new ENTITY.Direcciones.DireccionDTO
                                    {
                                        DireccionCompleta = reader.GetString(reader.GetOrdinal("DIRECCION_COMPLETA")),
                                        Ciudad = new ENTITY.Direcciones.CiudadDTO
                                        {
                                            Nombre = reader.GetString(reader.GetOrdinal("CIUDAD"))
                                        }
                                    }
                                });
                            }
                        }

                        return Response<PedidoDTO>.Done("Pedidos obtenidos exitosamente", default, pedidos);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<PedidoDTO>.Fail($"Error al obtener pedidos: {ex.Message}");
            }
        }
        public async Task<Response<PedidoCompletoDTO>> ObtenerPedidoCompleto(int idPedido)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_pedidos.sp_obtener_detalle_pedido";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_pedido", OracleDbType.Int32).Value = idPedido;

                        var cursorPedido = new OracleParameter("p_cursor_pedido", OracleDbType.RefCursor, ParameterDirection.Output);
                        var cursorDetalles = new OracleParameter("p_cursor_detalles", OracleDbType.RefCursor, ParameterDirection.Output);

                        command.Parameters.Add(cursorPedido);
                        command.Parameters.Add(cursorDetalles);

                        await command.ExecuteNonQueryAsync();

                        PedidoCompletoDTO pedido = null;

                        using (var reader = ((OracleRefCursor)cursorPedido.Value).GetDataReader())
                        {
                            if (await reader.ReadAsync())
                            {
                                pedido = new PedidoCompletoDTO
                                {
                                    IdPedido = reader.GetInt32(reader.GetOrdinal("ID_PEDIDO")),
                                    NumeroPedido = reader.GetString(reader.GetOrdinal("NUMERO_PEDIDO")),
                                    IdUsuario = reader.GetInt32(reader.GetOrdinal("ID_USUARIO")),
                                    FechaPedido = reader.GetDateTime(reader.GetOrdinal("FECHA_PEDIDO")),
                                    Estado = reader.GetString(reader.GetOrdinal("ESTADO")),
                                    Subtotal = reader.GetDecimal(reader.GetOrdinal("SUBTOTAL")),
                                    CostoEnvio = reader.GetDecimal(reader.GetOrdinal("COSTO_ENVIO")),
                                    Impuesto = reader.GetDecimal(reader.GetOrdinal("IMPUESTO")),
                                    Total = reader.GetDecimal(reader.GetOrdinal("TOTAL")),
                                    Cedula = reader.GetString(reader.GetOrdinal("CEDULA")),
                                    NombreCliente = reader.GetString(reader.GetOrdinal("NOMBRE_CLIENTE")),
                                    CorreoCliente = reader.GetString(reader.GetOrdinal("CORREO_CLIENTE")),
                                    TelefonoPrincipal = reader.GetString(reader.GetOrdinal("TELEFONO_PRINCIPAL")),
                                    DireccionCompleta = reader.GetString(reader.GetOrdinal("DIRECCION_COMPLETA")),
                                    Barrio = reader.GetString(reader.GetOrdinal("BARRIO")),
                                    Ciudad = reader.GetString(reader.GetOrdinal("CIUDAD")),
                                    Departamento = reader.GetString(reader.GetOrdinal("DEPARTAMENTO")),
                                    MetodoPago = reader.GetString(reader.GetOrdinal("METODO_PAGO"))
                                };
                            }
                        }

                        if (pedido == null)
                        {
                            return Response<PedidoCompletoDTO>.Fail("Pedido no encontrado");
                        }

                        using (var reader = ((OracleRefCursor)cursorDetalles.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                pedido.Productos.Add(new DetalleProductoDTO
                                {
                                    IdDetalle = reader.GetInt32(reader.GetOrdinal("ID_DETALLE")),
                                    Cantidad = reader.GetInt32(reader.GetOrdinal("CANTIDAD")),
                                    PrecioUnitario = reader.GetDecimal(reader.GetOrdinal("PRECIO_UNITARIO")),
                                    SubtotalLinea = reader.GetDecimal(reader.GetOrdinal("SUBTOTAL_LINEA")),
                                    NombreProducto = reader.GetString(reader.GetOrdinal("NOMBRE_PRODUCTO")),
                                    Marca = reader.GetString(reader.GetOrdinal("MARCA")),
                                    Talla = reader.GetString(reader.GetOrdinal("TALLA")),
                                    Color = reader.GetString(reader.GetOrdinal("COLOR")),
                                    CodigoSKU = reader.GetString(reader.GetOrdinal("CODIGO_SKU")),
                                    ImagenUrl = reader.IsDBNull(reader.GetOrdinal("URL_IMAGEN")) ? null : reader.GetString(reader.GetOrdinal("URL_IMAGEN"))
                                });
                            }
                        }

                        return Response<PedidoCompletoDTO>.Done("Pedido obtenido exitosamente", pedido);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<PedidoCompletoDTO>.Fail($"Error al obtener detalle del pedido: {ex.Message}");
            }
        }
        public async Task<Response<PedidoListaDTO>> ObtenerTodosPedidos()
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_pedidos.sp_obtener_todos_pedidos";
                        command.CommandType = CommandType.StoredProcedure;

                        var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                        command.Parameters.Add(cursorParam);

                        await command.ExecuteNonQueryAsync();

                        var pedidos = new List<PedidoListaDTO>();

                        using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                pedidos.Add(new PedidoListaDTO
                                {
                                    IdPedido = reader.GetInt32(reader.GetOrdinal("ID_PEDIDO")),
                                    NumeroPedido = reader.GetString(reader.GetOrdinal("NUMERO_PEDIDO")),
                                    FechaPedido = reader.GetDateTime(reader.GetOrdinal("FECHA_PEDIDO")),
                                    Estado = reader.GetString(reader.GetOrdinal("ESTADO")),
                                    Total = reader.GetDecimal(reader.GetOrdinal("TOTAL")),
                                    NombreCliente = reader.GetString(reader.GetOrdinal("NOMBRE_CLIENTE")),
                                    CorreoCliente = reader.GetString(reader.GetOrdinal("CORREO_CLIENTE")),
                                    MetodoPago = reader.GetString(reader.GetOrdinal("METODO_PAGO")),
                                    TotalProductos = reader.GetInt32(reader.GetOrdinal("TOTAL_PRODUCTOS"))
                                });
                            }
                        }

                        return Response<PedidoListaDTO>.Done("Pedidos obtenidos exitosamente", default, pedidos);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<PedidoListaDTO>.Fail($"Error al obtener pedidos: {ex.Message}");
            }
        }

        public async Task<Response<bool>> ActualizarEstadoPedido(ActualizarEstadoPedidoDTO actualizacion)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_pedidos.sp_actualizar_estado_pedido";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_pedido", OracleDbType.Int32).Value = actualizacion.IdPedido;
                        command.Parameters.Add("p_nuevo_estado", OracleDbType.Varchar2).Value = actualizacion.Estado;

                        await command.ExecuteNonQueryAsync();

                        return Response<bool>.Done("Estado actualizado exitosamente", true);
                    }
                }
            }
            catch (OracleException ex)
            {
                if (ex.Message.Contains("ORA-20020"))
                {
                    return Response<bool>.Fail("Pedido no encontrado");
                }
                if (ex.Message.Contains("ORA-20021"))
                {
                    return Response<bool>.Fail("Estado inválido");
                }

                return Response<bool>.Fail($"Error en Oracle: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<Response<bool>> CancelarPedido(int idPedido)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_pedidos.sp_cancelar_pedido";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_pedido", OracleDbType.Int32).Value = idPedido;

                        await command.ExecuteNonQueryAsync();

                        return Response<bool>.Done("Pedido cancelado exitosamente", true);
                    }
                }
            }
            catch (OracleException ex)
            {
                if (ex.Message.Contains("ORA-20030"))
                {
                    return Response<bool>.Fail("Solo se pueden cancelar pedidos Pendientes o Confirmados");
                }
                if (ex.Message.Contains("ORA-20031"))
                {
                    return Response<bool>.Fail("Pedido no encontrado");
                }

                return Response<bool>.Fail($"Error en Oracle: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<Response<MetodoPagoDTO>> ObtenerMetodosPago()
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_pedidos.sp_obtener_metodos_pago";
                        command.CommandType = CommandType.StoredProcedure;

                        var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                        command.Parameters.Add(cursorParam);

                        await command.ExecuteNonQueryAsync();

                        var metodos = new List<MetodoPagoDTO>();

                        using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                metodos.Add(new MetodoPagoDTO
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ID_METODO")),
                                    Nombre = reader.GetString(reader.GetOrdinal("NOMBRE"))
                                });
                            }
                        }

                        return Response<MetodoPagoDTO>.Done("Métodos de pago obtenidos exitosamente", default, metodos);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<MetodoPagoDTO>.Fail($"Error al obtener métodos de pago: {ex.Message}");
            }
        }
    }
}