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

                // Validar que exista al menos una imagen principal
                var tieneImagenPrincipal = articulo.Imagenes.Any(i => i.EsPrincipal == 'S');
                if (!tieneImagenPrincipal)
                {
                    return Response<int>.Fail("Debe marcar al menos una imagen como principal");
                }

                // Validar que no haya variantes duplicadas
                var variantesDuplicadas = articulo.Variantes
                    .GroupBy(v => new { v.Talla, v.Color })
                    .Where(g => g.Count() > 1)
                    .Select(g => $"{g.Key.Talla}-{g.Key.Color}")
                    .ToList();

                if (variantesDuplicadas.Any())
                {
                    return Response<int>.Fail($"Existen variantes duplicadas: {string.Join(", ", variantesDuplicadas)}");
                }

                // Validar que todas las variantes tengan stock >= 0
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
    }
}
