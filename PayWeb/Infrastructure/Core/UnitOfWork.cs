using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayWeb.Infrastructure.Core
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext dbContext;
        private IDbContextTransaction _transaction;

        public UnitOfWork(DbContext dbContext)
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
