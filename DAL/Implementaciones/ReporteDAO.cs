using DAL.Interfaces;
using ENTITY.Reportes;
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
    public class ReporteDAO : IReporteDAO
    {
        private readonly string _connectionString;

        public ReporteDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Response<ResumenFinancieroDTO>> ObtenerResumenFinancieroAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                using var command = new OracleCommand("pkg_reportes.sp_obtener_resumen_financiero", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_fecha_inicio", OracleDbType.Date).Value = fechaInicio;
                command.Parameters.Add("p_fecha_fin", OracleDbType.Date).Value = fechaFin;

                var cursorParam = command.Parameters.Add("p_cursor", OracleDbType.RefCursor);
                cursorParam.Direction = ParameterDirection.Output;

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                ResumenFinancieroDTO resumen = null;

                using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                {
                    if (await reader.ReadAsync())
                    {
                        resumen = new ResumenFinancieroDTO
                        {
                            TotalVentas = reader.GetDecimal(reader.GetOrdinal("TOTAL_VENTAS")),
                            TotalPedidos = reader.GetInt32(reader.GetOrdinal("TOTAL_PEDIDOS")),
                            TicketPromedio = reader.GetDecimal(reader.GetOrdinal("TICKET_PROMEDIO")),
                            TotalDescuentos = reader.GetDecimal(reader.GetOrdinal("TOTAL_DESCUENTOS")),
                            IngresosNetos = reader.GetDecimal(reader.GetOrdinal("INGRESOS_NETOS")),
                            IvaTotal = reader.GetDecimal(reader.GetOrdinal("IVA_TOTAL")),
                            SubtotalTotal = reader.GetDecimal(reader.GetOrdinal("SUBTOTAL_TOTAL")),
                            ClientesActivos = reader.GetInt32(reader.GetOrdinal("CLIENTES_ACTIVOS")),
                            ClientesNuevos = reader.GetInt32(reader.GetOrdinal("CLIENTES_NUEVOS"))
                        };
                    }
                }

                return resumen != null
                    ? Response<ResumenFinancieroDTO>.Done("Resumen financiero obtenido exitosamente", resumen)
                    : Response<ResumenFinancieroDTO>.Fail("No hay datos en el período seleccionado");
            }
            catch (OracleException ex)
            {
                return Response<ResumenFinancieroDTO>.Fail($"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<ResumenFinancieroDTO>.Fail($"Error al obtener resumen financiero: {ex.Message}");
            }
        }

        public async Task<Response<ProductoMasVendidoDTO>> ObtenerTopProductosAsync(DateTime fechaInicio, DateTime fechaFin, int limite = 10)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                using var command = new OracleCommand("pkg_reportes.sp_obtener_top_productos", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_fecha_inicio", OracleDbType.Date).Value = fechaInicio;
                command.Parameters.Add("p_fecha_fin", OracleDbType.Date).Value = fechaFin;
                command.Parameters.Add("p_limite", OracleDbType.Int32).Value = limite;

                var cursorParam = command.Parameters.Add("p_cursor", OracleDbType.RefCursor);
                cursorParam.Direction = ParameterDirection.Output;

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                var productos = new List<ProductoMasVendidoDTO>();

                using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                {
                    while (await reader.ReadAsync())
                    {
                        productos.Add(new ProductoMasVendidoDTO
                        {
                            IdArticulo = reader.GetInt32(reader.GetOrdinal("ID_ARTICULO")),
                            NombreProducto = reader.GetString(reader.GetOrdinal("NOMBRE_PRODUCTO")),
                            Marca = reader.GetString(reader.GetOrdinal("MARCA")),
                            CategoriaTipo = reader.GetString(reader.GetOrdinal("CATEGORIA_TIPO")),
                            CategoriaOcasion = reader.GetString(reader.GetOrdinal("CATEGORIA_OCASION")),
                            CantidadVendida = reader.GetInt32(reader.GetOrdinal("CANTIDAD_VENDIDA")),
                            IngresosGenerados = reader.GetDecimal(reader.GetOrdinal("INGRESOS_GENERADOS")),
                            PrecioPromedio = reader.GetDecimal(reader.GetOrdinal("PRECIO_PROMEDIO"))
                        });
                    }
                }

                return productos.Count > 0
                    ? Response<ProductoMasVendidoDTO>.Done($"Se encontraron {productos.Count} productos", list: productos)
                    : Response<ProductoMasVendidoDTO>.Fail("No hay productos vendidos en el período seleccionado");
            }
            catch (OracleException ex)
            {
                return Response<ProductoMasVendidoDTO>.Fail($"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<ProductoMasVendidoDTO>.Fail($"Error al obtener top productos: {ex.Message}");
            }
        }

        public async Task<Response<DetalleVentaDTO>> ObtenerDetalleVentasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                using var command = new OracleCommand("pkg_reportes.sp_obtener_detalle_ventas", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_fecha_inicio", OracleDbType.Date).Value = fechaInicio;
                command.Parameters.Add("p_fecha_fin", OracleDbType.Date).Value = fechaFin;

                var cursorParam = command.Parameters.Add("p_cursor", OracleDbType.RefCursor);
                cursorParam.Direction = ParameterDirection.Output;

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                var ventas = new List<DetalleVentaDTO>();

                using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                {
                    while (await reader.ReadAsync())
                    {
                        ventas.Add(new DetalleVentaDTO
                        {
                            IdPedido = reader.GetInt32(reader.GetOrdinal("ID_PEDIDO")),
                            NumeroPedido = reader.GetString(reader.GetOrdinal("NUMERO_PEDIDO")),
                            FechaPedido = reader.GetDateTime(reader.GetOrdinal("FECHA_PEDIDO")),
                            NombreCliente = reader.GetString(reader.GetOrdinal("NOMBRE_CLIENTE")),
                            CorreoCliente = reader.GetString(reader.GetOrdinal("CORREO_CLIENTE")),
                            MetodoPago = reader.GetString(reader.GetOrdinal("METODO_PAGO")),
                            Subtotal = reader.GetDecimal(reader.GetOrdinal("SUBTOTAL")),
                            Impuesto = reader.GetDecimal(reader.GetOrdinal("IMPUESTO")),
                            Descuento = reader.GetDecimal(reader.GetOrdinal("DESCUENTO")),
                            Total = reader.GetDecimal(reader.GetOrdinal("TOTAL")),
                            Estado = reader.GetString(reader.GetOrdinal("ESTADO")),
                            TieneFactura = reader.GetString(reader.GetOrdinal("TIENE_FACTURA"))
                        });
                    }
                }

                return ventas.Count > 0
                    ? Response<DetalleVentaDTO>.Done($"Se encontraron {ventas.Count} ventas", list: ventas)
                    : Response<DetalleVentaDTO>.Fail("No hay ventas en el período seleccionado");
            }
            catch (OracleException ex)
            {
                return Response<DetalleVentaDTO>.Fail($"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<DetalleVentaDTO>.Fail($"Error al obtener detalle de ventas: {ex.Message}");
            }
        }

        public async Task<Response<ResumenCuponDTO>> ObtenerResumenCuponesAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                using var command = new OracleCommand("pkg_reportes.sp_obtener_resumen_cupones", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_fecha_inicio", OracleDbType.Date).Value = fechaInicio;
                command.Parameters.Add("p_fecha_fin", OracleDbType.Date).Value = fechaFin;

                var cursorParam = command.Parameters.Add("p_cursor", OracleDbType.RefCursor);
                cursorParam.Direction = ParameterDirection.Output;

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                var cupones = new List<ResumenCuponDTO>();

                using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                {
                    while (await reader.ReadAsync())
                    {
                        cupones.Add(new ResumenCuponDTO
                        {
                            CodigoCupon = reader.GetString(reader.GetOrdinal("CODIGO_CUPON")),
                            Descripcion = reader.GetString(reader.GetOrdinal("DESCRIPCION")),
                            TipoDescuento = reader.GetString(reader.GetOrdinal("TIPO_DESCUENTO")),
                            ValorDescuento = reader.GetDecimal(reader.GetOrdinal("VALOR_DESCUENTO")),
                            VecesUsado = reader.GetInt32(reader.GetOrdinal("VECES_USADO")),
                            TotalDescontado = reader.GetDecimal(reader.GetOrdinal("TOTAL_DESCONTADO")),
                            DescuentoPromedio = reader.GetDecimal(reader.GetOrdinal("DESCUENTO_PROMEDIO"))
                        });
                    }
                }

                return cupones.Count > 0
                    ? Response<ResumenCuponDTO>.Done($"Se encontraron {cupones.Count} cupones utilizados", list: cupones)
                    : Response<ResumenCuponDTO>.Fail("No hay cupones utilizados en el período seleccionado");
            }
            catch (OracleException ex)
            {
                return Response<ResumenCuponDTO>.Fail($"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<ResumenCuponDTO>.Fail($"Error al obtener resumen de cupones: {ex.Message}");
            }
        }
    }
}
