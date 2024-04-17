using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayWeb.Common
{
    public class EntityResponse
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
        public static EntityResponse CreateOk()
        {
            return new EntityResponse
            {
                Ok = true
            };
        }
        public static EntityResponse<T> CreateOk<T>(T data)
        {
            return new EntityResponse<T>
            {
                Ok = true,
                Data = data
            };
        }
        public static EntityResponse CreateError(string mensaje)
        {
            return new EntityResponse
            {
                Ok = false,
                Mensaje = mensaje
            };
        }
        public static EntityResponse<T> CreateError<T>(string mensaje, T data = default(T))
        {
            return new EntityResponse<T>
            {
                Ok = false,
                Mensaje = mensaje,
                Data = data
            };
        }
    }

    public class EntityResponse<T> : EntityResponse
    {
        public T Data { get; set; }
    }
}
