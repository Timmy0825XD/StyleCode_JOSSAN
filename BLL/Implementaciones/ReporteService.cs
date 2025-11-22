using BLL.Interfaces;
using DAL.Interfaces;
using ENTITY.Reportes;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Implementaciones
{
    public class ReporteService : IReporteService
    {
        private readonly IReporteDAO _reporteDAO;

        public ReporteService(IReporteDAO reporteDAO)
        {
            _reporteDAO = reporteDAO;
        }

        public async Task<Response<ResumenFinancieroDTO>> ObtenerResumenFinancieroAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    return Response<ResumenFinancieroDTO>.Fail("La fecha de inicio no puede ser posterior a la fecha de fin");
                }

                var diasDiferencia = (fechaFin - fechaInicio).Days;
                if (diasDiferencia > 365)
                {
                    return Response<ResumenFinancieroDTO>.Fail("El rango de fechas no puede exceder 1 año");
                }

                if (fechaFin > DateTime.Now)
                {
                    return Response<ResumenFinancieroDTO>.Fail("No se pueden consultar fechas futuras");
                }

                return await _reporteDAO.ObtenerResumenFinancieroAsync(fechaInicio, fechaFin);
            }
            catch (Exception ex)
            {
                return Response<ResumenFinancieroDTO>.Fail($"Error al procesar la solicitud: {ex.Message}");
            }
        }

        public async Task<Response<ProductoMasVendidoDTO>> ObtenerTopProductosAsync(DateTime fechaInicio, DateTime fechaFin, int limite = 10)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    return Response<ProductoMasVendidoDTO>.Fail("La fecha de inicio no puede ser posterior a la fecha de fin");
                }

                if (limite < 1 || limite > 50)
                {
                    return Response<ProductoMasVendidoDTO>.Fail("El límite debe estar entre 1 y 50");
                }

                if (fechaFin > DateTime.Now)
                {
                    return Response<ProductoMasVendidoDTO>.Fail("No se pueden consultar fechas futuras");
                }

                return await _reporteDAO.ObtenerTopProductosAsync(fechaInicio, fechaFin, limite);
            }
            catch (Exception ex)
            {
                return Response<ProductoMasVendidoDTO>.Fail($"Error al procesar la solicitud: {ex.Message}");
            }
        }

        public async Task<Response<DetalleVentaDTO>> ObtenerDetalleVentasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    return Response<DetalleVentaDTO>.Fail("La fecha de inicio no puede ser posterior a la fecha de fin");
                }

                if (fechaFin > DateTime.Now)
                {
                    return Response<DetalleVentaDTO>.Fail("No se pueden consultar fechas futuras");
                }

                return await _reporteDAO.ObtenerDetalleVentasAsync(fechaInicio, fechaFin);
            }
            catch (Exception ex)
            {
                return Response<DetalleVentaDTO>.Fail($"Error al procesar la solicitud: {ex.Message}");
            }
        }

        public async Task<Response<ResumenCuponDTO>> ObtenerResumenCuponesAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    return Response<ResumenCuponDTO>.Fail("La fecha de inicio no puede ser posterior a la fecha de fin");
                }

                if (fechaFin > DateTime.Now)
                {
                    return Response<ResumenCuponDTO>.Fail("No se pueden consultar fechas futuras");
                }

                return await _reporteDAO.ObtenerResumenCuponesAsync(fechaInicio, fechaFin);
            }
            catch (Exception ex)
            {
                return Response<ResumenCuponDTO>.Fail($"Error al procesar la solicitud: {ex.Message}");
            }
        }
    }
}
