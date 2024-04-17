using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public IQueryable<T> Query()
        {
            return dbContext.Set<T>().AsQueryable();
        }
    }
}
