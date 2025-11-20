using DAL.Interfaces;
using DAL.Utilidades;
using ENTITY.Alertas;
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
    public class AlertaDAO : IAlertaDAO
    {
        private readonly OracleDbContext _context;

        public AlertaDAO(string connectionString)
        {
            _context = new OracleDbContext(connectionString);
        }

        public async Task<Response<AlertaStockDTO>> ObtenerAlertasPendientes()
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_alertas.obtener_alertas_pendientes", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var paramCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCursor);

                using var reader = await command.ExecuteReaderAsync();

                var listaAlertas = new List<AlertaStockDTO>();

                while (await reader.ReadAsync())
                {
                    listaAlertas.Add(new AlertaStockDTO
                    {
                        IdAlerta = reader.GetInt32(reader.GetOrdinal("id_alerta")),
                        IdVariante = reader.GetInt32(reader.GetOrdinal("id_variante")),
                        StockActual = reader.GetInt32(reader.GetOrdinal("stock_actual")),
                        FechaAlerta = reader.GetDateTime(reader.GetOrdinal("fecha_alerta")),
                        IdArticulo = reader.GetInt32(reader.GetOrdinal("id_articulo")),
                        NombreProducto = reader.GetString(reader.GetOrdinal("nombre_producto")),
                        Marca = reader.GetString(reader.GetOrdinal("marca")),
                        Talla = reader.GetString(reader.GetOrdinal("talla")),
                        Color = reader.GetString(reader.GetOrdinal("color")),
                        CodigoSku = reader.GetString(reader.GetOrdinal("codigo_sku")),
                        StockActualBd = reader.GetInt32(reader.GetOrdinal("stock_actual_bd")),
                        ImagenProducto = reader.IsDBNull(reader.GetOrdinal("imagen_producto")) ? null : reader.GetString(reader.GetOrdinal("imagen_producto")),
                        DiasPendiente = reader.GetInt32(reader.GetOrdinal("dias_pendiente")),
                        Estado = "Pendiente"
                    });
                }

                return Response<AlertaStockDTO>.Done("Alertas pendientes obtenidas exitosamente", list: listaAlertas);
            }
            catch (Exception ex)
            {
                return Response<AlertaStockDTO>.Fail($"Error al obtener alertas pendientes: {ex.Message}");
            }
        }

        public async Task<Response<AlertaStockDTO>> ObtenerTodasAlertas(string? estado = null)
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_alertas.obtener_todas_alertas", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var paramEstado = new OracleParameter("p_estado", OracleDbType.Varchar2)
                {
                    Direction = ParameterDirection.Input,
                    Value = string.IsNullOrEmpty(estado) ? DBNull.Value : (object)estado
                };
                command.Parameters.Add(paramEstado);

                var paramCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCursor);

                using var reader = await command.ExecuteReaderAsync();

                var listaAlertas = new List<AlertaStockDTO>();

                while (await reader.ReadAsync())
                {
                    listaAlertas.Add(new AlertaStockDTO
                    {
                        IdAlerta = reader.GetInt32(reader.GetOrdinal("id_alerta")),
                        IdVariante = reader.GetInt32(reader.GetOrdinal("id_variante")),
                        StockActual = reader.GetInt32(reader.GetOrdinal("stock_actual")),
                        FechaAlerta = reader.GetDateTime(reader.GetOrdinal("fecha_alerta")),
                        FechaResolucion = reader.IsDBNull(reader.GetOrdinal("fecha_resolucion")) ? null : reader.GetDateTime(reader.GetOrdinal("fecha_resolucion")),
                        Estado = reader.GetString(reader.GetOrdinal("estado")),
                        ResueltoPor = reader.IsDBNull(reader.GetOrdinal("resuelto_por")) ? null : reader.GetString(reader.GetOrdinal("resuelto_por")),
                        NombreProducto = reader.GetString(reader.GetOrdinal("nombre_producto")),
                        Marca = reader.GetString(reader.GetOrdinal("marca")),
                        Talla = reader.GetString(reader.GetOrdinal("talla")),
                        Color = reader.GetString(reader.GetOrdinal("color")),
                        CodigoSku = reader.GetString(reader.GetOrdinal("codigo_sku")),
                        StockActualBd = reader.GetInt32(reader.GetOrdinal("stock_actual_bd"))
                    });
                }

                return Response<AlertaStockDTO>.Done("Alertas obtenidas exitosamente", list: listaAlertas);
            }
            catch (Exception ex)
            {
                return Response<AlertaStockDTO>.Fail($"Error al obtener alertas: {ex.Message}");
            }
        }

        public async Task<Response<DetalleAlertaDTO>> ObtenerDetalleAlerta(int idAlerta)
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_alertas.obtener_detalle_alerta", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_id_alerta", OracleDbType.Int32, idAlerta, ParameterDirection.Input);

                var paramCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCursor);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var detalle = new DetalleAlertaDTO
                    {
                        IdAlerta = reader.GetInt32(reader.GetOrdinal("id_alerta")),
                        StockAlCrearAlerta = reader.GetInt32(reader.GetOrdinal("stock_al_crear_alerta")),
                        FechaAlerta = reader.GetDateTime(reader.GetOrdinal("fecha_alerta")),
                        FechaResolucion = reader.IsDBNull(reader.GetOrdinal("fecha_resolucion")) ? null : reader.GetDateTime(reader.GetOrdinal("fecha_resolucion")),
                        Estado = reader.GetString(reader.GetOrdinal("estado")),
                        ResueltoPor = reader.IsDBNull(reader.GetOrdinal("resuelto_por")) ? null : reader.GetString(reader.GetOrdinal("resuelto_por")),
                        IdArticulo = reader.GetInt32(reader.GetOrdinal("id_articulo")),
                        NombreProducto = reader.GetString(reader.GetOrdinal("nombre_producto")),
                        Marca = reader.GetString(reader.GetOrdinal("marca")),
                        PrecioBase = reader.GetDecimal(reader.GetOrdinal("precio_base")),
                        IdVariante = reader.GetInt32(reader.GetOrdinal("id_variante")),
                        Talla = reader.GetString(reader.GetOrdinal("talla")),
                        Color = reader.GetString(reader.GetOrdinal("color")),
                        CodigoSku = reader.GetString(reader.GetOrdinal("codigo_sku")),
                        StockActual = reader.GetInt32(reader.GetOrdinal("stock_actual")),
                        ImagenProducto = reader.IsDBNull(reader.GetOrdinal("imagen_producto")) ? null : reader.GetString(reader.GetOrdinal("imagen_producto")),
                        DiasResolucion = reader.GetInt32(reader.GetOrdinal("dias_resolucion"))
                    };

                    return Response<DetalleAlertaDTO>.Done("Detalle de alerta obtenido exitosamente", detalle);
                }

                return Response<DetalleAlertaDTO>.Fail("Alerta no encontrada");
            }
            catch (Exception ex)
            {
                return Response<DetalleAlertaDTO>.Fail($"Error al obtener detalle de alerta: {ex.Message}");
            }
        }
    }
}
