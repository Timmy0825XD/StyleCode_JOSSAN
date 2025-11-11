using ENTITY.Direcciones;
using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IDireccionDAO
    {
        Task<Response<int>> CrearDireccion(DireccionCreacionDTO direccion);
        Task<Response<bool>> ActualizarDireccion(DireccionActualizacionDTO direccion);
        Task<Response<DireccionDetalleDTO>> ObtenerDireccionPorId(int idDireccion);
        Task<Response<CiudadDTO>> ObtenerTodasLasCiudades();
    }
}
