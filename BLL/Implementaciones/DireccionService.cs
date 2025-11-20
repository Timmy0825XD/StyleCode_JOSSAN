using BLL.Interfaces;
using DAL.Interfaces;
using ENTITY.Direcciones;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Implementaciones
{
    public class DireccionService : IDireccionService
    {
        private readonly IDireccionDAO _direccionDAO;

        public DireccionService(IDireccionDAO direccionDAO)
        {
            _direccionDAO = direccionDAO;
        }

        public async Task<Response<int>> CrearDireccion(DireccionCreacionDTO direccion)
        {
            try
            {
                if (direccion.CiudadId <= 0)
                {
                    return Response<int>.Fail("Debe seleccionar una ciudad válida");
                }

                if (string.IsNullOrWhiteSpace(direccion.DireccionCompleta))
                {
                    return Response<int>.Fail("La dirección completa es requerida");
                }

                if (direccion.DireccionCompleta.Length < 10)
                {
                    return Response<int>.Fail("La dirección debe tener al menos 10 caracteres");
                }

                if (string.IsNullOrWhiteSpace(direccion.Barrio))
                {
                    return Response<int>.Fail("El barrio es requerido");
                }

                if (direccion.Barrio.Length < 3)
                {
                    return Response<int>.Fail("El barrio debe tener al menos 3 caracteres");
                }

                return await _direccionDAO.CrearDireccion(direccion);
            }
            catch (Exception ex)
            {
                return Response<int>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }

        public async Task<Response<bool>> ActualizarDireccion(DireccionActualizacionDTO direccion)
        {
            try
            {
                if (direccion.IdDireccion <= 0)
                {
                    return Response<bool>.Fail("ID de dirección inválido");
                }

                if (direccion.CiudadId <= 0)
                {
                    return Response<bool>.Fail("Debe seleccionar una ciudad válida");
                }

                if (string.IsNullOrWhiteSpace(direccion.DireccionCompleta))
                {
                    return Response<bool>.Fail("La dirección completa es requerida");
                }

                if (direccion.DireccionCompleta.Length < 10)
                {
                    return Response<bool>.Fail("La dirección debe tener al menos 10 caracteres");
                }

                if (string.IsNullOrWhiteSpace(direccion.Barrio))
                {
                    return Response<bool>.Fail("El barrio es requerido");
                }

                if (direccion.Barrio.Length < 3)
                {
                    return Response<bool>.Fail("El barrio debe tener al menos 3 caracteres");
                }

                return await _direccionDAO.ActualizarDireccion(direccion);
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }
        public async Task<Response<CiudadDTO>> ObtenerTodasLasCiudades()
        {
            try
            {
                return await _direccionDAO.ObtenerTodasLasCiudades();
            }
            catch (Exception ex)
            {
                return Response<CiudadDTO>.Fail($"Error en la capa de negocio: {ex.Message}");
            }
        }
    }
}
