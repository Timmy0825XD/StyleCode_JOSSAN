using ENTITY.Email;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IEmailDAO
    {
        Task<Response<ColaEmailDTO>> ObtenerEmailsPendientes();
        Task<Response<bool>> MarcarEmailEnviado(int idEmail);
        Task<Response<bool>> MarcarEmailError(int idEmail, string errorMensaje);
        Task<Response<DatosEmailPedidoDTO>> ObtenerDatosEmailPedido(int idPedido);
    }
}
