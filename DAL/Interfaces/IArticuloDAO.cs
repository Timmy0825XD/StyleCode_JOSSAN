using ENTITY.Articulos;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IArticuloDAO
    {
        Task<Response<int>> RegistrarArticuloCompleto(ArticuloCreacionDTO articulo);
        Task<Response<bool>> ActualizarArticulo(ArticuloActualizacionDTO articulo);
        Task<Response<ArticuloListaDTO>> ObtenerArticulosActivos();
        Task<Response<ArticuloDetalleDTO>> ObtenerArticuloPorId(int idArticulo);
        Task<Response<bool>> EliminarArticulo(int idArticulo);
        Task<Response<bool>> ActualizarVariante(VarianteActualizacionDTO variante);
        Task<Response<bool>> EliminarVariante(int idVariante);
        Task<Response<bool>> ActualizarImagen(ImagenActualizacionDTO imagen);
        Task<Response<bool>> EliminarImagen(int idImagen);
        Task<Response<bool>> EstablecerImagenPrincipal(int idArticulo, int idImagen);
    }
}
