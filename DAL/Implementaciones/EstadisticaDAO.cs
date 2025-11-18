using DAL.Interfaces;
using DAL.Utilidades;
using ENTITY.Estadisticas;
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
    public class EstadisticaDAO : IEstadisticaDAO
    {
        private readonly OracleDbContext _context;

        public EstadisticaDAO(string connectionString)
        {
            _context = new OracleDbContext(connectionString);
        }

        public async Task<Response<MetricasDashboardDTO>> ObtenerMetricasDashboard()
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_estadisticas.obtener_metricas_dashboard", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var paramCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCursor);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var metricas = new MetricasDashboardDTO
                    {
                        VentasMesActual = reader.GetDecimal(reader.GetOrdinal("ventas_mes_actual")),
                        VentasMesAnterior = reader.GetDecimal(reader.GetOrdinal("ventas_mes_anterior")),
                        VentasHoy = reader.GetDecimal(reader.GetOrdinal("ventas_hoy")),
                        TotalPedidosMes = reader.GetInt32(reader.GetOrdinal("total_pedidos_mes")),
                        PedidosPendientes = reader.GetInt32(reader.GetOrdinal("pedidos_pendientes")),
                        PedidosHoy = reader.GetInt32(reader.GetOrdinal("pedidos_hoy")),
                        TotalClientes = reader.GetInt32(reader.GetOrdinal("total_clientes")),
                        ClientesNuevosMes = reader.GetInt32(reader.GetOrdinal("clientes_nuevos_mes")),
                        TotalProductosActivos = reader.GetInt32(reader.GetOrdinal("total_productos_activos")),
                        ProductosStockBajo = reader.GetInt32(reader.GetOrdinal("productos_stock_bajo")),
                        TicketPromedio = reader.GetDecimal(reader.GetOrdinal("ticket_promedio")),
                        IvaMes = reader.GetDecimal(reader.GetOrdinal("iva_mes")),
                        TasaEntregaPorcentaje = reader.GetDecimal(reader.GetOrdinal("tasa_entrega_porcentaje"))
                    };

                    return Response<MetricasDashboardDTO>.Done("Métricas obtenidas exitosamente", metricas);
                }

                return Response<MetricasDashboardDTO>.Fail("No se pudieron obtener las métricas");
            }
            catch (Exception ex)
            {
                return Response<MetricasDashboardDTO>.Fail($"Error al obtener métricas: {ex.Message}");
            }
        }

        public async Task<Response<VentaMensualDTO>> ObtenerVentasMensuales(int anio)
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_estadisticas.obtener_ventas_mensuales", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_anio", OracleDbType.Int32, anio, ParameterDirection.Input);

                var paramCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCursor);

                using var reader = await command.ExecuteReaderAsync();

                var listaVentas = new List<VentaMensualDTO>();

                while (await reader.ReadAsync())
                {
                    listaVentas.Add(new VentaMensualDTO
                    {
                        MesNombre = reader.GetString(reader.GetOrdinal("mes_nombre")).Trim(),
                        MesNumero = reader.GetInt32(reader.GetOrdinal("mes_numero")),
                        TotalVentas = reader.GetDecimal(reader.GetOrdinal("total_ventas")),
                        CantidadPedidos = reader.GetInt32(reader.GetOrdinal("cantidad_pedidos"))
                    });
                }

                return Response<VentaMensualDTO>.Done("Ventas mensuales obtenidas exitosamente", list: listaVentas);
            }
            catch (Exception ex)
            {
                return Response<VentaMensualDTO>.Fail($"Error al obtener ventas mensuales: {ex.Message}");
            }
        }

        public async Task<Response<VentaCategoriaDTO>> ObtenerVentasPorCategoria()
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_estadisticas.obtener_ventas_por_categoria", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var paramCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCursor);

                using var reader = await command.ExecuteReaderAsync();

                var listaVentas = new List<VentaCategoriaDTO>();

                while (await reader.ReadAsync())
                {
                    listaVentas.Add(new VentaCategoriaDTO
                    {
                        Categoria = reader.GetString(reader.GetOrdinal("categoria")),
                        CantidadPedidos = reader.GetInt32(reader.GetOrdinal("cantidad_pedidos")),
                        UnidadesVendidas = reader.GetInt32(reader.GetOrdinal("unidades_vendidas")),
                        TotalVentas = reader.GetDecimal(reader.GetOrdinal("total_ventas")),
                        Porcentaje = reader.IsDBNull(reader.GetOrdinal("porcentaje")) ? 0 : reader.GetDecimal(reader.GetOrdinal("porcentaje"))
                    });
                }

                return Response<VentaCategoriaDTO>.Done("Ventas por categoría obtenidas exitosamente", list: listaVentas);
            }
            catch (Exception ex)
            {
                return Response<VentaCategoriaDTO>.Fail($"Error al obtener ventas por categoría: {ex.Message}");
            }
        }

        public async Task<Response<TopProductoDTO>> ObtenerTopProductos(int limite)
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_estadisticas.obtener_top_productos", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_limite", OracleDbType.Int32, limite, ParameterDirection.Input);

                var paramCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCursor);

                using var reader = await command.ExecuteReaderAsync();

                var listaProductos = new List<TopProductoDTO>();

                while (await reader.ReadAsync())
                {
                    listaProductos.Add(new TopProductoDTO
                    {
                        IdArticulo = reader.GetInt32(reader.GetOrdinal("id_articulo")),
                        Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                        Marca = reader.GetString(reader.GetOrdinal("marca")),
                        PrecioBase = reader.GetDecimal(reader.GetOrdinal("precio_base")),
                        UnidadesVendidas = reader.GetInt32(reader.GetOrdinal("unidades_vendidas")),
                        CantidadPedidos = reader.GetInt32(reader.GetOrdinal("cantidad_pedidos")),
                        IngresosGenerados = reader.GetDecimal(reader.GetOrdinal("ingresos_generados")),
                        Imagen = reader.IsDBNull(reader.GetOrdinal("imagen")) ? null : reader.GetString(reader.GetOrdinal("imagen"))
                    });
                }

                return Response<TopProductoDTO>.Done("Top productos obtenidos exitosamente", list: listaProductos);
            }
            catch (Exception ex)
            {
                return Response<TopProductoDTO>.Fail($"Error al obtener top productos: {ex.Message}");
            }
        }

        public async Task<Response<PedidoRecienteDTO>> ObtenerPedidosRecientes(int limite)
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_estadisticas.obtener_pedidos_recientes", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_limite", OracleDbType.Int32, limite, ParameterDirection.Input);

                var paramCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCursor);

                using var reader = await command.ExecuteReaderAsync();

                var listaPedidos = new List<PedidoRecienteDTO>();

                while (await reader.ReadAsync())
                {
                    listaPedidos.Add(new PedidoRecienteDTO
                    {
                        IdPedido = reader.GetInt32(reader.GetOrdinal("id_pedido")),
                        NumeroPedido = reader.GetString(reader.GetOrdinal("numero_pedido")),
                        FechaPedido = reader.GetDateTime(reader.GetOrdinal("fecha_pedido")),
                        Estado = reader.GetString(reader.GetOrdinal("estado")),
                        Total = reader.GetDecimal(reader.GetOrdinal("total")),
                        Cliente = reader.GetString(reader.GetOrdinal("cliente")),
                        EmailCliente = reader.GetString(reader.GetOrdinal("email_cliente")),
                        MetodoPago = reader.GetString(reader.GetOrdinal("metodo_pago")),
                        CantidadProductos = reader.GetInt32(reader.GetOrdinal("cantidad_productos"))
                    });
                }

                return Response<PedidoRecienteDTO>.Done("Pedidos recientes obtenidos exitosamente", list: listaPedidos);
            }
            catch (Exception ex)
            {
                return Response<PedidoRecienteDTO>.Fail($"Error al obtener pedidos recientes: {ex.Message}");
            }
        }

        public async Task<Response<PedidoPorEstadoDTO>> ObtenerPedidosPorEstado()
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_estadisticas.obtener_pedidos_por_estado", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var paramCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCursor);

                using var reader = await command.ExecuteReaderAsync();

                var listaPedidos = new List<PedidoPorEstadoDTO>();

                while (await reader.ReadAsync())
                {
                    listaPedidos.Add(new PedidoPorEstadoDTO
                    {
                        Estado = reader.GetString(reader.GetOrdinal("estado")),
                        Cantidad = reader.GetInt32(reader.GetOrdinal("cantidad")),
                        TotalVentas = reader.GetDecimal(reader.GetOrdinal("total_ventas")),
                        Porcentaje = reader.GetDecimal(reader.GetOrdinal("porcentaje"))
                    });
                }

                return Response<PedidoPorEstadoDTO>.Done("Pedidos por estado obtenidos exitosamente", list: listaPedidos);
            }
            catch (Exception ex)
            {
                return Response<PedidoPorEstadoDTO>.Fail($"Error al obtener pedidos por estado: {ex.Message}");
            }
        }

        public async Task<Response<TopClienteDTO>> ObtenerTopClientes(int limite)
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_estadisticas.obtener_top_clientes", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_limite", OracleDbType.Int32, limite, ParameterDirection.Input);

                var paramCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCursor);

                using var reader = await command.ExecuteReaderAsync();

                var listaClientes = new List<TopClienteDTO>();

                while (await reader.ReadAsync())
                {
                    listaClientes.Add(new TopClienteDTO
                    {
                        IdUsuario = reader.GetInt32(reader.GetOrdinal("id_usuario")),
                        NombreCompleto = reader.GetString(reader.GetOrdinal("nombre_completo")),
                        Correo = reader.GetString(reader.GetOrdinal("correo")),
                        TelefonoPrincipal = reader.GetString(reader.GetOrdinal("telefono_principal")),
                        TotalPedidos = reader.GetInt32(reader.GetOrdinal("total_pedidos")),
                        TotalGastado = reader.GetDecimal(reader.GetOrdinal("total_gastado")),
                        TicketPromedio = reader.GetDecimal(reader.GetOrdinal("ticket_promedio")),
                        UltimaCompra = reader.GetDateTime(reader.GetOrdinal("ultima_compra"))
                    });
                }

                return Response<TopClienteDTO>.Done("Top clientes obtenidos exitosamente", list: listaClientes);
            }
            catch (Exception ex)
            {
                return Response<TopClienteDTO>.Fail($"Error al obtener top clientes: {ex.Message}");
            }
        }

        public async Task<Response<ProductoStockBajoDTO>> ObtenerProductosStockBajo(int limite)
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_estadisticas.obtener_productos_stock_bajo", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_limite", OracleDbType.Int32, limite, ParameterDirection.Input);

                var paramCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCursor);

                using var reader = await command.ExecuteReaderAsync();

                var listaProductos = new List<ProductoStockBajoDTO>();

                while (await reader.ReadAsync())
                {
                    listaProductos.Add(new ProductoStockBajoDTO
                    {
                        IdArticulo = reader.GetInt32(reader.GetOrdinal("id_articulo")),
                        Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                        Marca = reader.GetString(reader.GetOrdinal("marca")),
                        Talla = reader.GetString(reader.GetOrdinal("talla")),
                        Color = reader.GetString(reader.GetOrdinal("color")),
                        CodigoSku = reader.GetString(reader.GetOrdinal("codigo_sku")),
                        Stock = reader.GetInt32(reader.GetOrdinal("stock")),
                        Imagen = reader.IsDBNull(reader.GetOrdinal("imagen")) ? null : reader.GetString(reader.GetOrdinal("imagen"))
                    });
                }

                return Response<ProductoStockBajoDTO>.Done("Productos con stock bajo obtenidos exitosamente", list: listaProductos);
            }
            catch (Exception ex)
            {
                return Response<ProductoStockBajoDTO>.Fail($"Error al obtener productos con stock bajo: {ex.Message}");
            }
        }

        public async Task<Response<VentaGeneroDTO>> ObtenerVentasPorGenero()
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_estadisticas.obtener_ventas_por_genero", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var paramCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCursor);

                using var reader = await command.ExecuteReaderAsync();

                var listaVentas = new List<VentaGeneroDTO>();

                while (await reader.ReadAsync())
                {
                    listaVentas.Add(new VentaGeneroDTO
                    {
                        Genero = reader.GetString(reader.GetOrdinal("genero")),
                        CantidadPedidos = reader.GetInt32(reader.GetOrdinal("cantidad_pedidos")),
                        UnidadesVendidas = reader.GetInt32(reader.GetOrdinal("unidades_vendidas")),
                        TotalVentas = reader.GetDecimal(reader.GetOrdinal("total_ventas")),
                        Porcentaje = reader.IsDBNull(reader.GetOrdinal("porcentaje")) ? 0 : reader.GetDecimal(reader.GetOrdinal("porcentaje"))
                    });
                }

                return Response<VentaGeneroDTO>.Done("Ventas por género obtenidas exitosamente", list: listaVentas);
            }
            catch (Exception ex)
            {
                return Response<VentaGeneroDTO>.Fail($"Error al obtener ventas por género: {ex.Message}");
            }
        }

        public async Task<Response<CrecimientoVentasDTO>> ObtenerCrecimientoVentas()
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_estadisticas.obtener_crecimiento_ventas", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var paramCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCursor);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var crecimiento = new CrecimientoVentasDTO
                    {
                        VentasMesActual = reader.GetDecimal(reader.GetOrdinal("ventas_mes_actual")),
                        VentasMesAnterior = reader.GetDecimal(reader.GetOrdinal("ventas_mes_anterior")),
                        CrecimientoPorcentajeMes = reader.IsDBNull(reader.GetOrdinal("crecimiento_porcentaje_mes")) ? 0 : reader.GetDecimal(reader.GetOrdinal("crecimiento_porcentaje_mes")),
                        VentasAnioActual = reader.GetDecimal(reader.GetOrdinal("ventas_anio_actual")),
                        VentasAnioAnterior = reader.GetDecimal(reader.GetOrdinal("ventas_anio_anterior")),
                        CrecimientoPorcentajeAnio = reader.IsDBNull(reader.GetOrdinal("crecimiento_porcentaje_anio")) ? 0 : reader.GetDecimal(reader.GetOrdinal("crecimiento_porcentaje_anio"))
                    };

                    return Response<CrecimientoVentasDTO>.Done("Crecimiento de ventas obtenido exitosamente", crecimiento);
                }

                return Response<CrecimientoVentasDTO>.Fail("No se pudo obtener el crecimiento de ventas");
            }
            catch (Exception ex)
            {
                return Response<CrecimientoVentasDTO>.Fail($"Error al obtener crecimiento de ventas: {ex.Message}");
            }
        }
    }
}
