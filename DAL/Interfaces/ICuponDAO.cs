using ENTITY.Cupones;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ICuponDAO
    {
        // Admin: Crear cupón
        Task<Response<int>> CrearCupon(CrearCuponDTO cupon, bool enviarEmailMasivo = false);

        // Admin: Obtener todos los cupones
        Task<Response<CuponDTO>> ObtenerTodosCupones();

        // Admin: Desactivar cupón
        Task<Response<bool>> DesactivarCupon(int idCupon);

        // Admin: Obtener historial de uso
        Task<Response<HistorialUsoCuponDTO>> ObtenerHistorialUso(int idCupon);

        // Cliente: Validar cupón
        Task<Response<ResultadoValidacionDTO>> ValidarCupon(ValidarCuponDTO validacion);

        // Cliente: Obtener cupones disponibles
        Task<Response<CuponDisponibleDTO>> ObtenerCuponesDisponibles(int idUsuario);

        // Sistema: Aplicar cupón a pedido
        Task<Response<bool>> AplicarCupon(AplicarCuponDTO aplicacion);
    }
}
