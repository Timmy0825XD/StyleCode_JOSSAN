using DAL.Interfaces;
using DAL.Utilidades;
using ENTITY.Favoritos;
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
    public class FavoritoDAO : IFavoritoDAO
    {
        private readonly OracleDbContext _context;

        public FavoritoDAO(string connectionString)
        {
            _context = new OracleDbContext(connectionString);
        }

        public async Task<Response<int>> AgregarFavorito(int idUsuario, int idArticulo)
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                using var command = new OracleCommand("pkg_favoritos.agregar_favorito", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    Transaction = transaction
                };

                // Parámetros de entrada
                command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
                command.Parameters.Add("p_id_articulo", OracleDbType.Int32, idArticulo, ParameterDirection.Input);

                // Parámetro de salida
                var paramIdFavorito = new OracleParameter("p_id_favorito_out", OracleDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramIdFavorito);

                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();

                int idFavoritoGenerado = Convert.ToInt32(paramIdFavorito.Value.ToString());

                return Response<int>.Done("Artículo agregado a favoritos exitosamente", idFavoritoGenerado);
            }
            catch (OracleException ex)
            {
                await transaction.RollbackAsync();

                return ex.Number switch
                {
                    20400 => Response<int>.Fail("Usuario no encontrado o inactivo"),
                    20401 => Response<int>.Fail("No se puede agregar a favoritos un artículo inactivo"),
                    20402 => Response<int>.Fail("Artículo no encontrado"),
                    20403 => Response<int>.Fail("Este artículo ya está en favoritos"),
                    _ => Response<int>.Fail($"Error al agregar favorito: {ex.Message}")
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Response<int>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<Response<bool>> EliminarFavorito(int idUsuario, int idArticulo)
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                using var command = new OracleCommand("pkg_favoritos.eliminar_favorito", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    Transaction = transaction
                };

                command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
                command.Parameters.Add("p_id_articulo", OracleDbType.Int32, idArticulo, ParameterDirection.Input);

                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();

                return Response<bool>.Done("Artículo eliminado de favoritos exitosamente");
            }
            catch (OracleException ex)
            {
                await transaction.RollbackAsync();

                return ex.Number switch
                {
                    20410 => Response<bool>.Fail("Este artículo no está en favoritos"),
                    _ => Response<bool>.Fail($"Error al eliminar favorito: {ex.Message}")
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Response<bool>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<Response<ToggleFavoritoResultDTO>> ToggleFavorito(int idUsuario, int idArticulo)
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                using var command = new OracleCommand("pkg_favoritos.toggle_favorito", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    Transaction = transaction
                };

                command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
                command.Parameters.Add("p_id_articulo", OracleDbType.Int32, idArticulo, ParameterDirection.Input);

                var paramAgregado = new OracleParameter("p_agregado", OracleDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramAgregado);

                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();

                int agregado = Convert.ToInt32(paramAgregado.Value.ToString());

                var resultado = new ToggleFavoritoResultDTO
                {
                    Agregado = agregado,
                    Mensaje = agregado == 1 ? "Artículo agregado a favoritos" : "Artículo eliminado de favoritos"
                };

                return Response<ToggleFavoritoResultDTO>.Done(resultado.Mensaje, resultado);
            }
            catch (OracleException ex)
            {
                await transaction.RollbackAsync();

                return ex.Number switch
                {
                    20420 => Response<ToggleFavoritoResultDTO>.Fail("Usuario no encontrado o inactivo"),
                    20421 => Response<ToggleFavoritoResultDTO>.Fail("No se puede agregar a favoritos un artículo inactivo"),
                    20422 => Response<ToggleFavoritoResultDTO>.Fail("Artículo no encontrado"),
                    _ => Response<ToggleFavoritoResultDTO>.Fail($"Error al alternar favorito: {ex.Message}")
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Response<ToggleFavoritoResultDTO>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<Response<int>> EsFavorito(int idUsuario, int idArticulo)
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_favoritos.es_favorito", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);
                command.Parameters.Add("p_id_articulo", OracleDbType.Int32, idArticulo, ParameterDirection.Input);

                var paramEsFavorito = new OracleParameter("p_es_favorito", OracleDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramEsFavorito);

                await command.ExecuteNonQueryAsync();

                int esFavorito = Convert.ToInt32(paramEsFavorito.Value.ToString());

                return Response<int>.Done("Consulta exitosa", esFavorito);
            }
            catch (Exception ex)
            {
                return Response<int>.Fail($"Error al verificar favorito: {ex.Message}");
            }
        }

        public async Task<Response<FavoritoDTO>> ObtenerFavoritosUsuario(int idUsuario)
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_favoritos.obtener_favoritos_usuario", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_id_usuario", OracleDbType.Int32, idUsuario, ParameterDirection.Input);

                var paramCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCursor);

                using var reader = await command.ExecuteReaderAsync();

                var listaFavoritos = new List<FavoritoDTO>();

                while (await reader.ReadAsync())
                {
                    listaFavoritos.Add(new FavoritoDTO
                    {
                        IdFavorito = reader.GetInt32(reader.GetOrdinal("id_favorito")),
                        IdArticulo = reader.GetInt32(reader.GetOrdinal("id_articulo")),
                        FechaAgregado = reader.GetDateTime(reader.GetOrdinal("fecha_agregado")),
                        Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                        Descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? string.Empty : reader.GetString(reader.GetOrdinal("descripcion")),
                        Marca = reader.GetString(reader.GetOrdinal("marca")),
                        Genero = reader.GetString(reader.GetOrdinal("genero")),
                        Material = reader.IsDBNull(reader.GetOrdinal("material")) ? string.Empty : reader.GetString(reader.GetOrdinal("material")),
                        PrecioBase = reader.GetDecimal(reader.GetOrdinal("precio_base")),
                        Estado = reader.GetString(reader.GetOrdinal("estado"))[0],
                        CategoriaTipo = reader.GetString(reader.GetOrdinal("categoria_tipo")),
                        CategoriaOcasion = reader.GetString(reader.GetOrdinal("categoria_ocasion")),
                        ImagenPrincipal = reader.IsDBNull(reader.GetOrdinal("imagen_principal")) ? null : reader.GetString(reader.GetOrdinal("imagen_principal")),
                        StockTotal = reader.GetInt32(reader.GetOrdinal("stock_total")),
                        VariantesDisponibles = reader.GetInt32(reader.GetOrdinal("variantes_disponibles"))
                    });
                }

                return Response<FavoritoDTO>.Done("Favoritos obtenidos exitosamente", list: listaFavoritos);
            }
            catch (Exception ex)
            {
                return Response<FavoritoDTO>.Fail($"Error al obtener favoritos: {ex.Message}");
            }
        }

        public async Task<Response<EstadisticaFavoritoDTO>> ObtenerEstadisticasFavoritos()
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            try
            {
                using var command = new OracleCommand("pkg_favoritos.obtener_estadisticas_favoritos", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var paramCursor = new OracleParameter("p_cursor", OracleDbType.RefCursor)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCursor);

                using var reader = await command.ExecuteReaderAsync();

                var listaEstadisticas = new List<EstadisticaFavoritoDTO>();

                while (await reader.ReadAsync())
                {
                    listaEstadisticas.Add(new EstadisticaFavoritoDTO
                    {
                        IdArticulo = reader.GetInt32(reader.GetOrdinal("id_articulo")),
                        Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                        Marca = reader.GetString(reader.GetOrdinal("marca")),
                        PrecioBase = reader.GetDecimal(reader.GetOrdinal("precio_base")),
                        VecesFavorito = reader.GetInt32(reader.GetOrdinal("veces_favorito")),
                        ImagenPrincipal = reader.IsDBNull(reader.GetOrdinal("imagen_principal")) ? null : reader.GetString(reader.GetOrdinal("imagen_principal")),
                        UltimoFavorito = reader.GetDateTime(reader.GetOrdinal("ultimo_favorito"))
                    });
                }

                return Response<EstadisticaFavoritoDTO>.Done("Estadísticas obtenidas exitosamente", list: listaEstadisticas);
            }
            catch (Exception ex)
            {
                return Response<EstadisticaFavoritoDTO>.Fail($"Error al obtener estadísticas: {ex.Message}");
            }
        }

        public async Task<Response<int>> LimpiarFavoritosInactivos()
        {
            using var connection = _context.CreateConnection() as OracleConnection;
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                using var command = new OracleCommand("pkg_favoritos.limpiar_favoritos_inactivos", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    Transaction = transaction
                };

                var paramCantidad = new OracleParameter("p_cantidad_eliminados", OracleDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(paramCantidad);

                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();

                int cantidadEliminados = Convert.ToInt32(paramCantidad.Value.ToString());

                return Response<int>.Done($"{cantidadEliminados} favorito(s) eliminado(s)", cantidadEliminados);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Response<int>.Fail($"Error al limpiar favoritos: {ex.Message}");
            }
        }
    }
}
