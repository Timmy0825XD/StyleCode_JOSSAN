using BLL.Interfaces;
using DAL.Interfaces;
using ENTITY.Articulos;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Implementaciones
{
    public class ArticuloServices : IArticuloServices
    {
        private readonly IArticuloDAO _articuloDAO;

        public ArticuloServices(IArticuloDAO articuloDAO)
        {
            _articuloDAO = articuloDAO;
        }

        public async Task<Response<int>> RegistrarArticuloCompleto(ArticuloCreacionDTO articulo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(articulo.Nombre))
                {
                    return Response<int>.Fail("El nombre del artículo es requerido");
                }

                if (string.IsNullOrWhiteSpace(articulo.Marca))
                {
                    return Response<int>.Fail("La marca es requerida");
                }

                if (articulo.PrecioBase <= 0)
                {
                    return Response<int>.Fail("El precio base debe ser mayor a 0");
                }

                if (articulo.Variantes == null || !articulo.Variantes.Any())
                {
                    return Response<int>.Fail("Debe agregar al menos una variante al artículo");
                }

                if (articulo.Imagenes == null || !articulo.Imagenes.Any())
                {
                    return Response<int>.Fail("Debe agregar al menos una imagen al artículo");
                }

                var tieneImagenPrincipal = articulo.Imagenes.Any(i => i.EsPrincipal == 'S');
                if (!tieneImagenPrincipal)
                {
                    return Response<int>.Fail("Debe marcar al menos una imagen como principal");
                }

                var variantesDuplicadas = articulo.Variantes
                    .GroupBy(v => new { v.Talla, v.Color })
                    .Where(g => g.Count() > 1)
                    .Select(g => $"{g.Key.Talla}-{g.Key.Color}")
                    .ToList();

                if (variantesDuplicadas.Any())
                {
                    return Response<int>.Fail($"Existen variantes duplicadas: {string.Join(", ", variantesDuplicadas)}");
                }

                var variantesConStockNegativo = articulo.Variantes.Where(v => v.Stock < 0).ToList();
                if (variantesConStockNegativo.Any())
                {
                    return Response<int>.Fail("El stock de las variantes no puede ser negativo");
                }

                return await _articuloDAO.RegistrarArticuloCompleto(articulo);
            }
            catch (Exception ex)
            {
                return Response<int>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<bool>> ActualizarArticulo(ArticuloActualizacionDTO articulo)
        {
            try
            {
                if (articulo.IdArticulo <= 0)
                {
                    return Response<bool>.Fail("ID de artículo inválido");
                }

                if (string.IsNullOrWhiteSpace(articulo.Nombre))
                {
                    return Response<bool>.Fail("El nombre del artículo es requerido");
                }

                if (string.IsNullOrWhiteSpace(articulo.Marca))
                {
                    return Response<bool>.Fail("La marca es requerida");
                }

                if (articulo.PrecioBase <= 0)
                {
                    return Response<bool>.Fail("El precio base debe ser mayor a 0");
                }

                return await _articuloDAO.ActualizarArticulo(articulo);
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<bool>> ActualizarStock(ActualizarStockDTO stockDTO)
        {
            try
            {
                if (stockDTO.IdArticulo <= 0)
                {
                    return Response<bool>.Fail("ID de artículo inválido");
                }

                if (string.IsNullOrWhiteSpace(stockDTO.Talla))
                {
                    return Response<bool>.Fail("La talla es requerida");
                }

                if (string.IsNullOrWhiteSpace(stockDTO.Color))
                {
                    return Response<bool>.Fail("El color es requerido");
                }

                if (stockDTO.NuevoStock < 0)
                {
                    return Response<bool>.Fail("El stock no puede ser negativo");
                }

                return await _articuloDAO.ActualizarStock(stockDTO);
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<ArticuloListaDTO>> ObtenerArticulosActivos()
        {
            try
            {
                return await _articuloDAO.ObtenerArticulosActivos();
            }
            catch (Exception ex)
            {
                return Response<ArticuloListaDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<ArticuloDetalleDTO>> ObtenerArticuloPorId(int idArticulo)
        {
            try
            {
                if (idArticulo <= 0)
                {
                    return Response<ArticuloDetalleDTO>.Fail("ID de artículo inválido");
                }

                return await _articuloDAO.ObtenerArticuloPorId(idArticulo);
            }
            catch (Exception ex)
            {
                return Response<ArticuloDetalleDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<bool>> EliminarArticulo(int idArticulo)
        {
            try
            {
                if (idArticulo <= 0)
                {
                    return Response<bool>.Fail("ID de artículo inválido");
                }

                return await _articuloDAO.EliminarArticulo(idArticulo);
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }
        public async Task<Response<bool>> ActualizarVariante(VarianteActualizacionDTO variante)
        {
            try
            {
                // Validaciones
                if (variante.IdVariante == null || variante.IdVariante <= 0)
                {
                    return Response<bool>.Fail("ID de variante inválido");
                }

                if (string.IsNullOrWhiteSpace(variante.Talla))
                {
                    return Response<bool>.Fail("La talla es requerida");
                }

                if (string.IsNullOrWhiteSpace(variante.Color))
                {
                    return Response<bool>.Fail("El color es requerido");
                }

                if (variante.Stock < 0)
                {
                    return Response<bool>.Fail("El stock no puede ser negativo");
                }

                if (variante.Estado != 'A' && variante.Estado != 'I')
                {
                    return Response<bool>.Fail("Estado inválido. Use 'A' para activo o 'I' para inactivo");
                }

                return await _articuloDAO.ActualizarVariante(variante);
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<bool>> EliminarVariante(int idVariante)
        {
            try
            {
                if (idVariante <= 0)
                {
                    return Response<bool>.Fail("ID de variante inválido");
                }

                return await _articuloDAO.EliminarVariante(idVariante);
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<bool>> ActualizarImagen(ImagenActualizacionDTO imagen)
        {
            try
            {
                // Validaciones
                if (imagen.IdImagen == null || imagen.IdImagen <= 0)
                {
                    return Response<bool>.Fail("ID de imagen inválido");
                }

                if (string.IsNullOrWhiteSpace(imagen.Url))
                {
                    return Response<bool>.Fail("La URL de la imagen es requerida");
                }

                if (imagen.Orden <= 0)
                {
                    return Response<bool>.Fail("El orden debe ser mayor a 0");
                }

                if (imagen.EsPrincipal != 'S' && imagen.EsPrincipal != 'N')
                {
                    return Response<bool>.Fail("EsPrincipal debe ser 'S' o 'N'");
                }

                // Validar formato URL (opcional pero recomendado)
                if (!Uri.TryCreate(imagen.Url, UriKind.Absolute, out Uri? uriResult) ||
                    (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    return Response<bool>.Fail("La URL de la imagen no es válida");
                }

                return await _articuloDAO.ActualizarImagen(imagen);
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<bool>> EliminarImagen(int idImagen)
        {
            try
            {
                if (idImagen <= 0)
                {
                    return Response<bool>.Fail("ID de imagen inválido");
                }

                return await _articuloDAO.EliminarImagen(idImagen);
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<bool>> EstablecerImagenPrincipal(int idArticulo, int idImagen)
        {
            try
            {
                if (idArticulo <= 0)
                {
                    return Response<bool>.Fail("ID de artículo inválido");
                }

                if (idImagen <= 0)
                {
                    return Response<bool>.Fail("ID de imagen inválido");
                }

                return await _articuloDAO.EstablecerImagenPrincipal(idArticulo, idImagen);
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }
    }
}
