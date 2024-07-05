using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using PayWeb.Infrastructure.Core;
using System.Threading.Tasks;

namespace CRM.Infrastructure.Core
{
    public class UnitOfWorkPayroll : IUnitOfWorkPayroll
    {
        private readonly DbContext dbContext;
        private IDbContextTransaction _transaction;

        public UnitOfWorkPayroll(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task BeginTransactionAsync()
        {
            _transaction = await dbContext.Database.BeginTransactionAsync();
        }
        public async Task CommitAsync()
        {
            await _transaction.CommitAsync();
            _transaction.Dispose();
            _transaction = null;
        }
        public IRepository<T> Repository<T>() where T : class
        {
            return new Repository<T>(dbContext);
        }
        public async Task RollBackAsync()
        {
            await _transaction.RollbackAsync();
            _transaction.Dispose();
            _transaction = null;
        }
        public async Task<int> SaveChangesAsync()
        {
            var saved = 0;
            if (_transaction == null)
            {
                try
                {
                    await BeginTransactionAsync();
                    saved = await dbContext.SaveChangesAsync();
                    await CommitAsync();
                    return saved;
                }
                catch (System.Exception ex)
                {
                    await RollBackAsync();
                    throw ex;
                }
            }
            return await dbContext.SaveChangesAsync();
        }
    }
}
