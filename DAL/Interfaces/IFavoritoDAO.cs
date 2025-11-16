using ENTITY.Favoritos;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IFavoritoDAO
    {
        Task<Response<int>> AgregarFavorito(int idUsuario, int idArticulo);
        Task<Response<bool>> EliminarFavorito(int idUsuario, int idArticulo);
        Task<Response<ToggleFavoritoResultDTO>> ToggleFavorito(int idUsuario, int idArticulo);
        Task<Response<int>> EsFavorito(int idUsuario, int idArticulo);
        Task<Response<FavoritoDTO>> ObtenerFavoritosUsuario(int idUsuario);
        Task<Response<EstadisticaFavoritoDTO>> ObtenerEstadisticasFavoritos();
        Task<Response<int>> LimpiarFavoritosInactivos();
    }
}
