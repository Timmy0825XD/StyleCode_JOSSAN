using ENTITY.Reportes;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IReporteDAO
    {
        Task<Response<ResumenFinancieroDTO>> ObtenerResumenFinancieroAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<Response<ProductoMasVendidoDTO>> ObtenerTopProductosAsync(DateTime fechaInicio, DateTime fechaFin, int limite = 10);
        Task<Response<DetalleVentaDTO>> ObtenerDetalleVentasAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<Response<ResumenCuponDTO>> ObtenerResumenCuponesAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}
