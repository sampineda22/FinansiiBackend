using PayWeb.Infrastructure.Core;
using System.Threading.Tasks;

namespace CRM.Infrastructure.Core
{
    public interface IUnitOfWorkGira
    {
        IRepository<T> Repository<T>() where T : class;


        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task RollBackAsync();
        Task CommitAsync();
    }
}
