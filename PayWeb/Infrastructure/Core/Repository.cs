using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PayWeb.Infrastructure.Core
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DbContext dbContext;
        public Repository(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public void Add(T entity)
        {
            dbContext.Set<T>().Add(entity);
        }
        public void Add(IEnumerable<T> entities)
        {
            dbContext.Set<T>().AddRange(entities);
        }

        public List<T> GetSP<T>(string query, SqlParameter[] paramsArray, int commandTimeout = 100) where T : class, new()
        {
            var result = new List<T>();
            dbContext.Database.OpenConnection();
            var command = dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddRange(paramsArray);
            command.CommandTimeout = commandTimeout;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                   
                    T t = new T();

                    for (int inc = 0; inc < reader.FieldCount; inc++)
                    {
                        Type type = t.GetType();
                        PropertyInfo propInfo = type.GetProperty(reader.GetName(inc));
                        if (propInfo != null)
                        {
                            object value = reader.GetValue(inc);
                            if (value == DBNull.Value)
                            {
                                propInfo.SetValue(t, null);
                            }
                            else
                            {
                                propInfo.SetValue(t, value);
                            }
                        }
                    }

                    result.Add(t);
                }
            }


            return result;
        }


       

        public int ExecuteSP(string query, SqlParameter[] paramsArray) 
        {
            int ExecutionResult = 0;
            //var result = new List<T>();
            dbContext.Database.OpenConnection();
            var command = dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddRange(paramsArray);

            ExecutionResult = command.ExecuteNonQuery();

           
            return ExecutionResult;
        }

        public IQueryable<T> Query()
        {
            return dbContext.Set<T>().AsQueryable();
        }

        public void Update(T entity)
        {
            dbContext.Set<T>().Update(entity);
        }

        public void Delete(T entity)
        {
            dbContext.Set<T>().Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            dbContext.Set<T>().RemoveRange(entities);
        }

        public int ExecuteSPT(string query, SqlParameter parameter)
        {
            int ExecutionResult = 0;
            //var result = new List<T>();
            dbContext.Database.OpenConnection();
            var command = dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(parameter);

            ExecutionResult = command.ExecuteNonQuery();


            return ExecutionResult;
        }
    }

    public static class MyExtensions
    {

        
    }


}
