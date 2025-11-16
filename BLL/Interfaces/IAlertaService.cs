using ENTITY.Alertas;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IAlertaService
    {
        Task<Response<AlertaStockDTO>> ObtenerAlertasPendientes();
        Task<Response<AlertaStockDTO>> ObtenerTodasAlertas(string? estado = null);
        Task<Response<DetalleAlertaDTO>> ObtenerDetalleAlerta(int idAlerta);
        Task<Response<EstadisticaAlertaDTO>> ObtenerEstadisticasAlertas();
    }
}
