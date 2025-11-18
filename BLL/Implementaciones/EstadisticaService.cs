using BLL.Interfaces;
using DAL.Interfaces;
using ENTITY.Estadisticas;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Implementaciones
{
    public class EstadisticaService : IEstadisticaService
    {
        private readonly IEstadisticaDAO _estadisticaDAO;

        public EstadisticaService(IEstadisticaDAO estadisticaDAO)
        {
            _estadisticaDAO = estadisticaDAO;
        }

        public async Task<Response<MetricasDashboardDTO>> ObtenerMetricasDashboard()
        {
            try
            {
                return await _estadisticaDAO.ObtenerMetricasDashboard();
            }
            catch (Exception ex)
            {
                return Response<MetricasDashboardDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<VentaMensualDTO>> ObtenerVentasMensuales(int anio)
        {
            try
            {
                // Validar año
                if (anio < 2000 || anio > 2100)
                {
                    return Response<VentaMensualDTO>.Fail("Año inválido. Debe estar entre 2000 y 2100");
                }

                return await _estadisticaDAO.ObtenerVentasMensuales(anio);
            }
            catch (Exception ex)
            {
                return Response<VentaMensualDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<VentaCategoriaDTO>> ObtenerVentasPorCategoria()
        {
            try
            {
                return await _estadisticaDAO.ObtenerVentasPorCategoria();
            }
            catch (Exception ex)
            {
                return Response<VentaCategoriaDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<TopProductoDTO>> ObtenerTopProductos(int limite = 10)
        {
            try
            {
                if (limite <= 0)
                {
                    return Response<TopProductoDTO>.Fail("El límite debe ser mayor a 0");
                }

                if (limite > 100)
                {
                    return Response<TopProductoDTO>.Fail("El límite no puede ser mayor a 100");
                }

                return await _estadisticaDAO.ObtenerTopProductos(limite);
            }
            catch (Exception ex)
            {
                return Response<TopProductoDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<PedidoRecienteDTO>> ObtenerPedidosRecientes(int limite = 10)
        {
            try
            {
                if (limite <= 0)
                {
                    return Response<PedidoRecienteDTO>.Fail("El límite debe ser mayor a 0");
                }

                if (limite > 100)
                {
                    return Response<PedidoRecienteDTO>.Fail("El límite no puede ser mayor a 100");
                }

                return await _estadisticaDAO.ObtenerPedidosRecientes(limite);
            }
            catch (Exception ex)
            {
                return Response<PedidoRecienteDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<PedidoPorEstadoDTO>> ObtenerPedidosPorEstado()
        {
            try
            {
                return await _estadisticaDAO.ObtenerPedidosPorEstado();
            }
            catch (Exception ex)
            {
                return Response<PedidoPorEstadoDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<TopClienteDTO>> ObtenerTopClientes(int limite = 10)
        {
            try
            {
                if (limite <= 0)
                {
                    return Response<TopClienteDTO>.Fail("El límite debe ser mayor a 0");
                }

                if (limite > 100)
                {
                    return Response<TopClienteDTO>.Fail("El límite no puede ser mayor a 100");
                }

                return await _estadisticaDAO.ObtenerTopClientes(limite);
            }
            catch (Exception ex)
            {
                return Response<TopClienteDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<ProductoStockBajoDTO>> ObtenerProductosStockBajo(int limite = 10)
        {
            try
            {
                if (limite <= 0)
                {
                    return Response<ProductoStockBajoDTO>.Fail("El límite debe ser mayor a 0");
                }

                if (limite > 100)
                {
                    return Response<ProductoStockBajoDTO>.Fail("El límite no puede ser mayor a 100");
                }

                return await _estadisticaDAO.ObtenerProductosStockBajo(limite);
            }
            catch (Exception ex)
            {
                return Response<ProductoStockBajoDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<VentaGeneroDTO>> ObtenerVentasPorGenero()
        {
            try
            {
                return await _estadisticaDAO.ObtenerVentasPorGenero();
            }
            catch (Exception ex)
            {
                return Response<VentaGeneroDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<CrecimientoVentasDTO>> ObtenerCrecimientoVentas()
        {
            try
            {
                return await _estadisticaDAO.ObtenerCrecimientoVentas();
            }
            catch (Exception ex)
            {
                return Response<CrecimientoVentasDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }
    }
}
