using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayWeb.Infrastructure.Core
{
    public interface IUnitOfWork
    {
        IRepository<T> Repository<T>() where T : class;
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task RollBackAsync();
        Task CommitAsync();
    }
}
