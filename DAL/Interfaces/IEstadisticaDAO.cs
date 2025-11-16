using ENTITY.Estadisticas;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IEstadisticaDAO
    {
        Task<Response<MetricasDashboardDTO>> ObtenerMetricasDashboard();
        Task<Response<VentaMensualDTO>> ObtenerVentasMensuales(int anio);
        Task<Response<VentaCategoriaDTO>> ObtenerVentasPorCategoria();
        Task<Response<TopProductoDTO>> ObtenerTopProductos(int limite);
        Task<Response<PedidoRecienteDTO>> ObtenerPedidosRecientes(int limite);
        Task<Response<PedidoPorEstadoDTO>> ObtenerPedidosPorEstado();
        Task<Response<TopClienteDTO>> ObtenerTopClientes(int limite);
        Task<Response<ProductoStockBajoDTO>> ObtenerProductosStockBajo(int limite);
        Task<Response<VentaGeneroDTO>> ObtenerVentasPorGenero();
        Task<Response<CrecimientoVentasDTO>> ObtenerCrecimientoVentas();
    }
}
