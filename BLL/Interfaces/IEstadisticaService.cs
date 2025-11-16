using ENTITY.Estadisticas;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IEstadisticaService
    {
        Task<Response<MetricasDashboardDTO>> ObtenerMetricasDashboard();
        Task<Response<VentaMensualDTO>> ObtenerVentasMensuales(int anio);
        Task<Response<VentaCategoriaDTO>> ObtenerVentasPorCategoria();
        Task<Response<TopProductoDTO>> ObtenerTopProductos(int limite = 10);
        Task<Response<PedidoRecienteDTO>> ObtenerPedidosRecientes(int limite = 10);
        Task<Response<PedidoPorEstadoDTO>> ObtenerPedidosPorEstado();
        Task<Response<TopClienteDTO>> ObtenerTopClientes(int limite = 10);
        Task<Response<ProductoStockBajoDTO>> ObtenerProductosStockBajo(int limite = 10);
        Task<Response<VentaGeneroDTO>> ObtenerVentasPorGenero();
        Task<Response<CrecimientoVentasDTO>> ObtenerCrecimientoVentas();
    }
}
