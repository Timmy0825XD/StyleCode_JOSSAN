using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Utilidades
{
    public class Response<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Object { get; set; }
        public IList<T> ListObject { get; set; }

        public Response(bool isSuccess, string message, T obj, IList<T> list)
        {
            IsSuccess = isSuccess;
            Message = message;
            Object = obj;
            ListObject = list;
        }

        public static Response<T> Done(string message = "Operación exitosa", T obj = default, IList<T> list = null)
        {
            return new Response<T>(true, message, obj, list);
        }

        public static Response<T> Fail(string message = "Ocurrió un error", T obj = default, IList<T> list = null)
        {
            return new Response<T>(false, message, obj, list);
        }
    }
}
