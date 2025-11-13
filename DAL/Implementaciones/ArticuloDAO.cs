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
    public class ArticuloDAO : IArticuloDAO
    {
        private readonly string _connectionString;

        public ArticuloDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Response<int>> RegistrarArticuloCompleto(ArticuloCreacionDTO articulo)
        {
            OracleConnection connection = null;
            OracleTransaction transaction = null;

            try
            {
                connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                transaction = connection.BeginTransaction();

                int idArticuloGenerado = 0;

                // Registrar el artículo base
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "pkg_articulos.registrar_articulo";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("p_id_categoria_tipo", OracleDbType.Int32).Value = articulo.CategoriaTipoId;
                    command.Parameters.Add("p_id_categoria_ocasion", OracleDbType.Int32).Value = articulo.CategoriaOcasionId;
                    command.Parameters.Add("p_nombre", OracleDbType.Varchar2).Value = articulo.Nombre;
                    command.Parameters.Add("p_descripcion", OracleDbType.Varchar2).Value = articulo.Descripcion ?? (object)DBNull.Value;
                    command.Parameters.Add("p_marca", OracleDbType.Varchar2).Value = articulo.Marca;
                    command.Parameters.Add("p_genero", OracleDbType.Varchar2).Value = articulo.Genero;
                    command.Parameters.Add("p_material", OracleDbType.Varchar2).Value = articulo.Material ?? (object)DBNull.Value;
                    command.Parameters.Add("p_precio_base", OracleDbType.Decimal).Value = articulo.PrecioBase;
                    command.Parameters.Add("p_estado", OracleDbType.Char).Value = articulo.Estado;

                    var idParam = new OracleParameter("p_id_articulo_generado", OracleDbType.Int32, ParameterDirection.Output);
                    command.Parameters.Add(idParam);

                    await command.ExecuteNonQueryAsync();

                    idArticuloGenerado = ((OracleDecimal)idParam.Value).ToInt32();
                }

                // Registrar variantes
                foreach (var variante in articulo.Variantes)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = "pkg_articulos.registrar_variante";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_articulo", OracleDbType.Int32).Value = idArticuloGenerado;
                        command.Parameters.Add("p_talla", OracleDbType.Varchar2).Value = variante.Talla;
                        command.Parameters.Add("p_color", OracleDbType.Varchar2).Value = variante.Color;
                        command.Parameters.Add("p_stock", OracleDbType.Int32).Value = variante.Stock;

                        var idVarianteParam = new OracleParameter("p_id_variante_generada", OracleDbType.Int32, ParameterDirection.Output);
                        command.Parameters.Add(idVarianteParam);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                // Registrar imágenes

                var imagenesOrdenadas = articulo.Imagenes.OrderBy(i => i.Orden).ToList();

                foreach (var imagen in imagenesOrdenadas)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = "pkg_articulos.registrar_imagen";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_articulo", OracleDbType.Int32).Value = idArticuloGenerado;
                        command.Parameters.Add("p_url_imagen", OracleDbType.Varchar2).Value = imagen.Url;
                        command.Parameters.Add("p_orden", OracleDbType.Int32).Value = imagen.Orden;
                        command.Parameters.Add("p_es_principal", OracleDbType.Char).Value = imagen.EsPrincipal;

                        var idImagenParam = new OracleParameter("p_id_imagen_generada", OracleDbType.Int32, ParameterDirection.Output);
                        command.Parameters.Add(idImagenParam);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();

                return Response<int>.Done("Artículo registrado exitosamente", idArticuloGenerado);
            }
            catch (OracleException ex)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();

                if (ex.Message.Contains("ORA-20003"))
                {
                    return Response<int>.Fail("Ya existe una variante con esa combinación de talla y color");
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

        public async Task<Response<bool>> ActualizarArticulo(ArticuloActualizacionDTO articulo)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_articulos.actualizar_articulo";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_articulo", OracleDbType.Int32).Value = articulo.IdArticulo;
                        command.Parameters.Add("p_nombre", OracleDbType.Varchar2).Value = articulo.Nombre;
                        command.Parameters.Add("p_descripcion", OracleDbType.Varchar2).Value = articulo.Descripcion ?? (object)DBNull.Value;
                        command.Parameters.Add("p_marca", OracleDbType.Varchar2).Value = articulo.Marca;
                        command.Parameters.Add("p_genero", OracleDbType.Varchar2).Value = articulo.Genero;
                        command.Parameters.Add("p_material", OracleDbType.Varchar2).Value = articulo.Material ?? (object)DBNull.Value;
                        command.Parameters.Add("p_precio_base", OracleDbType.Decimal).Value = articulo.PrecioBase;
                        command.Parameters.Add("p_estado", OracleDbType.Char).Value = articulo.Estado;

                        await command.ExecuteNonQueryAsync();

                        return Response<bool>.Done("Artículo actualizado exitosamente", true);
                    }
                }
            }
            catch (OracleException ex)
            {
                if (ex.Message.Contains("ORA-20008"))
                {
                    return Response<bool>.Fail("No se encontró el artículo para actualizar");
                }
                return Response<bool>.Fail($"Error en Oracle: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<Response<bool>> ActualizarStock(ActualizarStockDTO stockDTO)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_articulos.actualizar_stock_variante";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_articulo", OracleDbType.Int32).Value = stockDTO.IdArticulo;
                        command.Parameters.Add("p_talla", OracleDbType.Varchar2).Value = stockDTO.Talla;
                        command.Parameters.Add("p_color", OracleDbType.Varchar2).Value = stockDTO.Color;
                        command.Parameters.Add("p_nuevo_stock", OracleDbType.Int32).Value = stockDTO.NuevoStock;

                        await command.ExecuteNonQueryAsync();

                        return Response<bool>.Done("Stock actualizado exitosamente", true);
                    }
                }
            }
            catch (OracleException ex)
            {
                if (ex.Message.Contains("ORA-20006"))
                {
                    return Response<bool>.Fail("No existe una variante con los parámetros especificados");
                }
                return Response<bool>.Fail($"Error en Oracle: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<Response<ArticuloListaDTO>> ObtenerArticulosActivos()
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_articulos.listar_articulos_activos";
                        command.CommandType = CommandType.StoredProcedure;

                        var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                        command.Parameters.Add(cursorParam);

                        await command.ExecuteNonQueryAsync();

                        var articulos = new List<ArticuloListaDTO>();

                        using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                var articulo = new ArticuloListaDTO
                                {
                                    IdArticulo = reader.GetInt32(reader.GetOrdinal("ID_ARTICULO")),
                                    Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
                                    Descripcion = reader.IsDBNull(reader.GetOrdinal("DESCRIPCION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPCION")),
                                    Marca = reader.GetString(reader.GetOrdinal("MARCA")),
                                    Genero = reader.GetString(reader.GetOrdinal("GENERO")),
                                    Material = reader.IsDBNull(reader.GetOrdinal("MATERIAL")) ? null : reader.GetString(reader.GetOrdinal("MATERIAL")),
                                    PrecioBase = reader.GetDecimal(reader.GetOrdinal("PRECIO_BASE")),
                                    Estado = reader.GetString(reader.GetOrdinal("ESTADO")),
                                    FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
                                    CategoriaTipo = reader.GetString(reader.GetOrdinal("CATEGORIA_TIPO")),
                                    CategoriaOcasion = reader.GetString(reader.GetOrdinal("CATEGORIA_OCASION")),
                                    TotalVariantes = reader.GetInt32(reader.GetOrdinal("TOTAL_VARIANTES")),
                                    ImagenPrincipal = reader.IsDBNull(reader.GetOrdinal("IMAGEN_PRINCIPAL")) ? null : reader.GetString(reader.GetOrdinal("IMAGEN_PRINCIPAL"))
                                };

                                articulos.Add(articulo);
                            }
                        }

                        return Response<ArticuloListaDTO>.Done("Artículos obtenidos exitosamente", list: articulos);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<ArticuloListaDTO>.Fail($"Error al obtener artículos: {ex.Message}");
            }
        }

        public async Task<Response<ArticuloDetalleDTO>> ObtenerArticuloPorId(int idArticulo)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_articulos.obtener_articulo_por_id";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_articulo", OracleDbType.Int32).Value = idArticulo;

                        var cursorArticulo = new OracleParameter("p_cursor_articulo", OracleDbType.RefCursor, ParameterDirection.Output);
                        var cursorVariantes = new OracleParameter("p_cursor_variantes", OracleDbType.RefCursor, ParameterDirection.Output);
                        var cursorImagenes = new OracleParameter("p_cursor_imagenes", OracleDbType.RefCursor, ParameterDirection.Output);

                        command.Parameters.Add(cursorArticulo);
                        command.Parameters.Add(cursorVariantes);
                        command.Parameters.Add(cursorImagenes);

                        await command.ExecuteNonQueryAsync();

                        ArticuloDetalleDTO articulo = null;

                        // Leer artículo
                        using (var reader = ((OracleRefCursor)cursorArticulo.Value).GetDataReader())
                        {
                            if (await reader.ReadAsync())
                            {
                                articulo = new ArticuloDetalleDTO
                                {
                                    IdArticulo = reader.GetInt32(reader.GetOrdinal("ID_ARTICULO")),
                                    Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
                                    Descripcion = reader.IsDBNull(reader.GetOrdinal("DESCRIPCION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPCION")),
                                    Marca = reader.GetString(reader.GetOrdinal("MARCA")),
                                    Genero = reader.GetString(reader.GetOrdinal("GENERO")),
                                    Material = reader.IsDBNull(reader.GetOrdinal("MATERIAL")) ? null : reader.GetString(reader.GetOrdinal("MATERIAL")),
                                    PrecioBase = reader.GetDecimal(reader.GetOrdinal("PRECIO_BASE")),
                                    Estado = reader.GetString(reader.GetOrdinal("ESTADO")),
                                    FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
                                    CategoriaTipoId = reader.GetInt32(reader.GetOrdinal("ID_CATEGORIA_TIPO")),
                                    CategoriaOcasionId = reader.GetInt32(reader.GetOrdinal("ID_CATEGORIA_OCASION")),
                                    CategoriaTipo = reader.GetString(reader.GetOrdinal("CATEGORIA_TIPO")),
                                    CategoriaOcasion = reader.GetString(reader.GetOrdinal("CATEGORIA_OCASION"))
                                };
                            }
                        }

                        if (articulo == null)
                        {
                            return Response<ArticuloDetalleDTO>.Fail("Artículo no encontrado");
                        }

                        // Leer variantes
                        using (var reader = ((OracleRefCursor)cursorVariantes.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                var variante = new VarianteDetalleDTO
                                {
                                    IdVariante = reader.GetInt32(reader.GetOrdinal("ID_VARIANTE")),
                                    Talla = reader.GetString(reader.GetOrdinal("TALLA")),
                                    Color = reader.GetString(reader.GetOrdinal("COLOR")),
                                    CodigoSKU = reader.GetString(reader.GetOrdinal("CODIGO_SKU")),
                                    Stock = reader.GetInt32(reader.GetOrdinal("STOCK")),
                                    Estado = reader.GetString(reader.GetOrdinal("ESTADO"))
                                };

                                articulo.Variantes.Add(variante);
                            }
                        }

                        // Leer imágenes
                        using (var reader = ((OracleRefCursor)cursorImagenes.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                var imagen = new ImagenArticuloDTO
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ID_IMAGEN")),
                                    Url = reader.GetString(reader.GetOrdinal("URL_IMAGEN")),
                                    Orden = reader.GetInt32(reader.GetOrdinal("ORDEN")),
                                    EsPrincipal = reader.GetString(reader.GetOrdinal("ES_PRINCIPAL"))[0]
                                };

                                articulo.Imagenes.Add(imagen);
                            }
                        }

                        return Response<ArticuloDetalleDTO>.Done("Artículo obtenido exitosamente", articulo);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<ArticuloDetalleDTO>.Fail($"Error al obtener artículo: {ex.Message}");
            }
        }

        public async Task<Response<bool>> EliminarArticulo(int idArticulo)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_articulos.eliminar_articulo";
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_id_articulo", OracleDbType.Int32).Value = idArticulo;

                        await command.ExecuteNonQueryAsync();

                        return Response<bool>.Done("Artículo eliminado exitosamente", true);
                    }
                }
            }
            catch (OracleException ex)
            {
                if (ex.Message.Contains("ORA-20010"))
                {
                    return Response<bool>.Fail("El artículo no existe o ya está inactivo");
                }
                return Response<bool>.Fail($"Error en Oracle: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error inesperado: {ex.Message}");
            }
        }
    }
}
