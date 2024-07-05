using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayWeb.Infrastructure.Core
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> Query();
        void Add(T entity);
        void Add(IEnumerable<T> entities);
        List<T> GetSP<T>(string query, SqlParameter[] paramsArray, int commandTimeout = 100) where T : class, new();
        int ExecuteSP(string query, SqlParameter[] paramsArray);
        //Ver despues
        int ExecuteSPT(string query, SqlParameter parameter);
        void Delete(T entity);

        void Update(T entity);
        void DeleteRange(IEnumerable<T> entities);
    }
}
