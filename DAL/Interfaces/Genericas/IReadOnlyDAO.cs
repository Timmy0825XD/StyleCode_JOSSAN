using ENTITY.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces.Genericas
{
    public interface IReadOnlyDAO<T> where T : class
    {
        Task<Response<T>> ObtenerTodos();
        Task<Response<T>> ObtenerPorId(int id);
    }
}
