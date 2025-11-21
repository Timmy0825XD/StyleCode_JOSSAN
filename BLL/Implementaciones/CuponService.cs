using BLL.Interfaces;
using DAL.Interfaces;
using ENTITY.Cupones;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Implementaciones
{
    public class CuponService : ICuponService
    {
        private readonly ICuponDAO _cuponDAO;

        public CuponService(ICuponDAO cuponDAO)
        {
            _cuponDAO = cuponDAO;
        }

        public async Task<Response<int>> CrearCupon(CrearCuponDTO cupon, bool enviarEmailMasivo = false)
        {
            // Validaciones de negocio
            if (string.IsNullOrWhiteSpace(cupon.Codigo))
                return Response<int>.Fail("El código del cupón es obligatorio");

            if (cupon.Codigo.Length < 3 || cupon.Codigo.Length > 20)
                return Response<int>.Fail("El código debe tener entre 3 y 20 caracteres");

            if (string.IsNullOrWhiteSpace(cupon.Descripcion))
                return Response<int>.Fail("La descripción es obligatoria");

            if (cupon.ValorDescuento <= 0)
                return Response<int>.Fail("El valor del descuento debe ser mayor a 0");

            if (cupon.TipoDescuento == "PORCENTAJE" && cupon.ValorDescuento > 100)
                return Response<int>.Fail("El porcentaje no puede ser mayor a 100");

            if (cupon.UsosMaximos.HasValue && cupon.UsosMaximos.Value <= 0)
                return Response<int>.Fail("Los usos máximos deben ser mayor a 0");

            return await _cuponDAO.CrearCupon(cupon, enviarEmailMasivo);
        }

        public async Task<Response<CuponDTO>> ObtenerTodosCupones()
        {
            return await _cuponDAO.ObtenerTodosCupones();
        }

        public async Task<Response<bool>> DesactivarCupon(int idCupon)
        {
            if (idCupon <= 0)
                return Response<bool>.Fail("ID de cupón inválido");

            return await _cuponDAO.DesactivarCupon(idCupon);
        }

        public async Task<Response<HistorialUsoCuponDTO>> ObtenerHistorialUso(int idCupon)
        {
            if (idCupon <= 0)
                return Response<HistorialUsoCuponDTO>.Fail("ID de cupón inválido");

            return await _cuponDAO.ObtenerHistorialUso(idCupon);
        }

        public async Task<Response<ResultadoValidacionDTO>> ValidarCupon(ValidarCuponDTO validacion)
        {
            if (string.IsNullOrWhiteSpace(validacion.Codigo))
                return Response<ResultadoValidacionDTO>.Fail("El código del cupón es obligatorio");

            if (validacion.IdUsuario <= 0)
                return Response<ResultadoValidacionDTO>.Fail("Usuario inválido");

            if (validacion.Subtotal <= 0)
                return Response<ResultadoValidacionDTO>.Fail("Subtotal inválido");

            return await _cuponDAO.ValidarCupon(validacion);
        }

        public async Task<Response<CuponDisponibleDTO>> ObtenerCuponesDisponibles(int idUsuario)
        {
            if (idUsuario <= 0)
                return Response<CuponDisponibleDTO>.Fail("Usuario inválido");

            return await _cuponDAO.ObtenerCuponesDisponibles(idUsuario);
        }

        public async Task<Response<bool>> AplicarCupon(AplicarCuponDTO aplicacion)
        {
            if (aplicacion.IdPedido <= 0)
                return Response<bool>.Fail("Pedido inválido");

            if (string.IsNullOrWhiteSpace(aplicacion.CodigoCupon))
                return Response<bool>.Fail("Código de cupón es obligatorio");

            if (aplicacion.IdUsuario <= 0)
                return Response<bool>.Fail("Usuario inválido");

            return await _cuponDAO.AplicarCupon(aplicacion);
        }
    }
}
