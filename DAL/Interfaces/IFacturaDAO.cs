using ENTITY.Facturas;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IFacturaDAO
    {
        Task<Response<int>> CrearFactura(
            int idPedido,
            int idUsuario,
            string numeroFactura,
            string cufe,
            string codigoQr,
            string estadoDian,
            DateTime? fechaDian
        );

        Task<Response<FacturaDTO>> ObtenerTodasFacturas();
        Task<Response<FacturaDTO>> ObtenerFacturaPorId(int idFactura);
        Task<Response<FacturaDTO>> ObtenerFacturasUsuario(int idUsuario);
        Task<Response<FacturaDTO>> ObtenerFacturaPorNumero(string numeroFactura);
        Task<Response<FacturaDTO>> ObtenerFacturaPorPedido(int idPedido);
        Task<Response<bool>> ActualizarEstado(int idFactura, string nuevoEstado);
        Task<Response<bool>> ActualizarEstadoDian(int idFactura, string estadoDian, DateTime fechaDian);
        Task<Response<VerificarFacturaResult>> VerificarFacturaPedido(int idPedido);
    }

    public class VerificarFacturaResult
    {
        public bool Existe { get; set; }
        public int IdFactura { get; set; }
    }
}
