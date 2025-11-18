using BLL.Interfaces;
using DAL.Interfaces;
using ENTITY.Alertas;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Implementaciones
{
    public class AlertaService : IAlertaService
    {
        private readonly IAlertaDAO _alertaDAO;

        public AlertaService(IAlertaDAO alertaDAO)
        {
            _alertaDAO = alertaDAO;
        }
        public async Task<Response<AlertaStockDTO>> ObtenerAlertasPendientes()
        {
            try
            {
                return await _alertaDAO.ObtenerAlertasPendientes();
            }
            catch (Exception ex)
            {
                return Response<AlertaStockDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<AlertaStockDTO>> ObtenerTodasAlertas(string? estado = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(estado))
                {
                    if (estado != "Pendiente" && estado != "Resuelta")
                    {
                        return Response<AlertaStockDTO>.Fail("Estado inválido. Debe ser 'Pendiente' o 'Resuelta'");
                    }
                }

                return await _alertaDAO.ObtenerTodasAlertas(estado);
            }
            catch (Exception ex)
            {
                return Response<AlertaStockDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }
        public async Task<Response<DetalleAlertaDTO>> ObtenerDetalleAlerta(int idAlerta)
        {
            try
            {
                if (idAlerta <= 0)
                {
                    return Response<DetalleAlertaDTO>.Fail("ID de alerta inválido");
                }

                return await _alertaDAO.ObtenerDetalleAlerta(idAlerta);
            }
            catch (Exception ex)
            {
                return Response<DetalleAlertaDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<EstadisticaAlertaDTO>> ObtenerEstadisticasAlertas()
        {
            try
            {
                return await _alertaDAO.ObtenerEstadisticasAlertas();
            }
            catch (Exception ex)
            {
                return Response<EstadisticaAlertaDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }
    }
}
