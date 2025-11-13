using ENTITY.Facturas;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IFacturaService
    {
        Task<Response<int>> GenerarFacturaElectronica(int idPedido);
        Task<Response<FacturaDTO>> ObtenerTodasFacturas();
        Task<Response<FacturaDTO>> ObtenerFacturaPorId(int idFactura);
        Task<Response<FacturaDTO>> ObtenerFacturasUsuario(int idUsuario);
        Task<Response<FacturaDTO>> ObtenerFacturaPorPedido(int idPedido);
        Task<Response<bool>> ExisteFacturaParaPedido(int idPedido);
    }
}
