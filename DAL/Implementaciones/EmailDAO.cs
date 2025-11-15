using DAL.Interfaces;
using ENTITY.Email;
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
    public class EmailDAO : IEmailDAO
    {
        private readonly string _connectionString;

        public EmailDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Response<ColaEmailDTO>> ObtenerEmailsPendientes()
        {
            var listaEmails = new List<ColaEmailDTO>();

            try
            {
                using var conexion = new OracleConnection(_connectionString);
                await conexion.OpenAsync();

                using var comando = new OracleCommand("pkg_emails.obtener_emails_pendientes", conexion)
                {
                    CommandType = CommandType.StoredProcedure
                };

                comando.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

                using var reader = await comando.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    listaEmails.Add(new ColaEmailDTO
                    {
                        IdEmail = reader.GetInt32(0),
                        TipoEmail = reader.GetString(1),
                        IdPedido = reader.GetInt32(2),
                        Destinatario = reader.GetString(3),
                        Asunto = reader.GetString(4),
                        Intentos = reader.GetInt32(5),
                        FechaCreacion = reader.GetDateTime(6)
                    });
                }

                return Response<ColaEmailDTO>.Done("Emails obtenidos correctamente", list: listaEmails);
            }
            catch (Exception ex)
            {
                return Response<ColaEmailDTO>.Fail($"Error al obtener emails pendientes: {ex.Message}");
            }
        }

        public async Task<Response<bool>> MarcarEmailEnviado(int idEmail)
        {
            try
            {
                using var conexion = new OracleConnection(_connectionString);
                await conexion.OpenAsync();

                using var comando = new OracleCommand("pkg_emails.marcar_email_enviado", conexion)
                {
                    CommandType = CommandType.StoredProcedure
                };

                comando.Parameters.Add("p_id_email", OracleDbType.Int32, idEmail, ParameterDirection.Input);

                await comando.ExecuteNonQueryAsync();
                return Response<bool>.Done("Email marcado como enviado");
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error al marcar email como enviado: {ex.Message}");
            }
        }

        public async Task<Response<bool>> MarcarEmailError(int idEmail, string errorMensaje)
        {
            try
            {
                using var conexion = new OracleConnection(_connectionString);
                await conexion.OpenAsync();

                using var comando = new OracleCommand("pkg_emails.marcar_email_error", conexion)
                {
                    CommandType = CommandType.StoredProcedure
                };

                comando.Parameters.Add("p_id_email", OracleDbType.Int32, idEmail, ParameterDirection.Input);
                comando.Parameters.Add("p_error_mensaje", OracleDbType.Varchar2, errorMensaje, ParameterDirection.Input);

                await comando.ExecuteNonQueryAsync();
                return Response<bool>.Done("Email marcado con error");
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error al marcar email con error: {ex.Message}");
            }
        }

        public async Task<Response<DatosEmailPedidoDTO>> ObtenerDatosEmailPedido(int idPedido)
        {
            try
            {
                using var conexion = new OracleConnection(_connectionString);
                await conexion.OpenAsync();

                using var comando = new OracleCommand("pkg_emails.obtener_datos_email_pedido", conexion)
                {
                    CommandType = CommandType.StoredProcedure
                };

                comando.Parameters.Add("p_id_pedido", OracleDbType.Int32, idPedido, ParameterDirection.Input);
                comando.Parameters.Add("p_cursor_pedido", OracleDbType.RefCursor, ParameterDirection.Output);
                comando.Parameters.Add("p_cursor_productos", OracleDbType.RefCursor, ParameterDirection.Output);

                using var reader = await comando.ExecuteReaderAsync();

                DatosEmailPedidoDTO? datosPedido = null;

                if (await reader.ReadAsync())
                {
                    datosPedido = new DatosEmailPedidoDTO
                    {
                        NumeroPedido = reader.GetString(0),
                        FechaPedido = reader.GetDateTime(1),
                        Estado = reader.GetString(2),
                        Subtotal = reader.GetDecimal(3),
                        CostoEnvio = reader.GetDecimal(4),
                        Impuesto = reader.GetDecimal(5),
                        Total = reader.GetDecimal(6),
                        NombreCliente = reader.GetString(7),
                        Correo = reader.GetString(8),
                        DireccionCompleta = reader.GetString(9),
                        Barrio = reader.GetString(10),
                        Ciudad = reader.GetString(11),
                        Departamento = reader.GetString(12),
                        MetodoPago = reader.GetString(13)
                    };
                }

                if (datosPedido == null)
                {
                    return Response<DatosEmailPedidoDTO>.Fail("No se encontraron datos del pedido");
                }

                await reader.NextResultAsync();
                while (await reader.ReadAsync())
                {
                    datosPedido.Productos.Add(new ProductoEmailDTO
                    {
                        NombreProducto = reader.GetString(0),
                        Marca = reader.GetString(1),
                        Talla = reader.GetString(2),
                        Color = reader.GetString(3),
                        Cantidad = reader.GetInt32(4),
                        PrecioUnitario = reader.GetDecimal(5),
                        SubtotalLinea = reader.GetDecimal(6),
                        UrlImagen = reader.IsDBNull(7) ? null : reader.GetString(7)
                    });
                }

                return Response<DatosEmailPedidoDTO>.Done("Datos obtenidos correctamente", datosPedido);
            }
            catch (Exception ex)
            {
                return Response<DatosEmailPedidoDTO>.Fail($"Error al obtener datos del pedido: {ex.Message}");
            }
        }
    }
}
