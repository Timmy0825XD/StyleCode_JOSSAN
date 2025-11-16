using BLL.Interfaces;
using DAL.Interfaces;
using ENTITY.Favoritos;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Implementaciones
{
    public class FavoritoService : IFavoritoService
    {
        private readonly IFavoritoDAO _favoritoDAO;

        public FavoritoService(IFavoritoDAO favoritoDAO)
        {
            _favoritoDAO = favoritoDAO;
        }

        // ========================================
        // AGREGAR FAVORITO
        // ========================================
        public async Task<Response<int>> AgregarFavorito(int idUsuario, int idArticulo)
        {
            try
            {
                if (idUsuario <= 0)
                {
                    return Response<int>.Fail("ID de usuario inválido");
                }

                if (idArticulo <= 0)
                {
                    return Response<int>.Fail("ID de artículo inválido");
                }

                return await _favoritoDAO.AgregarFavorito(idUsuario, idArticulo);
            }
            catch (Exception ex)
            {
                return Response<int>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        // ========================================
        // ELIMINAR FAVORITO
        // ========================================
        public async Task<Response<bool>> EliminarFavorito(int idUsuario, int idArticulo)
        {
            try
            {
                if (idUsuario <= 0)
                {
                    return Response<bool>.Fail("ID de usuario inválido");
                }

                if (idArticulo <= 0)
                {
                    return Response<bool>.Fail("ID de artículo inválido");
                }

                return await _favoritoDAO.EliminarFavorito(idUsuario, idArticulo);
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        // ========================================
        // TOGGLE FAVORITO
        // ========================================
        public async Task<Response<ToggleFavoritoResultDTO>> ToggleFavorito(int idUsuario, int idArticulo)
        {
            try
            {
                if (idUsuario <= 0)
                {
                    return Response<ToggleFavoritoResultDTO>.Fail("ID de usuario inválido");
                }

                if (idArticulo <= 0)
                {
                    return Response<ToggleFavoritoResultDTO>.Fail("ID de artículo inválido");
                }

                return await _favoritoDAO.ToggleFavorito(idUsuario, idArticulo);
            }
            catch (Exception ex)
            {
                return Response<ToggleFavoritoResultDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        // ========================================
        // ES FAVORITO
        // ========================================
        public async Task<Response<int>> EsFavorito(int idUsuario, int idArticulo)
        {
            try
            {
                if (idUsuario <= 0)
                {
                    return Response<int>.Fail("ID de usuario inválido");
                }

                if (idArticulo <= 0)
                {
                    return Response<int>.Fail("ID de artículo inválido");
                }

                return await _favoritoDAO.EsFavorito(idUsuario, idArticulo);
            }
            catch (Exception ex)
            {
                return Response<int>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        // ========================================
        // OBTENER FAVORITOS DEL USUARIO
        // ========================================
        public async Task<Response<FavoritoDTO>> ObtenerFavoritosUsuario(int idUsuario)
        {
            try
            {
                if (idUsuario <= 0)
                {
                    return Response<FavoritoDTO>.Fail("ID de usuario inválido");
                }

                return await _favoritoDAO.ObtenerFavoritosUsuario(idUsuario);
            }
            catch (Exception ex)
            {
                return Response<FavoritoDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        // ========================================
        // OBTENER ESTADÍSTICAS DE FAVORITOS
        // ========================================
        public async Task<Response<EstadisticaFavoritoDTO>> ObtenerEstadisticasFavoritos()
        {
            try
            {
                return await _favoritoDAO.ObtenerEstadisticasFavoritos();
            }
            catch (Exception ex)
            {
                return Response<EstadisticaFavoritoDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        // ========================================
        // LIMPIAR FAVORITOS INACTIVOS
        // ========================================
        public async Task<Response<int>> LimpiarFavoritosInactivos()
        {
            try
            {
                return await _favoritoDAO.LimpiarFavoritosInactivos();
            }
            catch (Exception ex)
            {
                return Response<int>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }
    }
}
