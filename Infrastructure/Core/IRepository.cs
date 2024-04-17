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
    }
}
