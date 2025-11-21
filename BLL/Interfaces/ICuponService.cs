using ENTITY.Cupones;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ICuponService
    {

        Task<Response<int>> CrearCupon(CrearCuponDTO cupon, bool enviarEmailMasivo = false);
        Task<Response<CuponDTO>> ObtenerTodosCupones();

        Task<Response<bool>> DesactivarCupon(int idCupon);

        Task<Response<HistorialUsoCuponDTO>> ObtenerHistorialUso(int idCupon);

        Task<Response<ResultadoValidacionDTO>> ValidarCupon(ValidarCuponDTO validacion);

        Task<Response<CuponDisponibleDTO>> ObtenerCuponesDisponibles(int idUsuario);

        Task<Response<bool>> AplicarCupon(AplicarCuponDTO aplicacion);
    }
}
