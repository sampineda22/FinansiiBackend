using CRM.Infrastructure.Maps.Gira;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Context
{
    public class GiraContext : DbContext
    {
        public GiraContext(DbContextOptions<GiraContext> optionsBuilder) : base(optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new ExpenseTypeMap());
            modelBuilder.ApplyConfiguration(new ExpenseCategoryMap());
            modelBuilder.ApplyConfiguration(new ExpenseAccountMap());
            modelBuilder.ApplyConfiguration(new TaxGroupMap());
            modelBuilder.ApplyConfiguration(new UserMap());
            modelBuilder.ApplyConfiguration(new FuelTypeMap());
            modelBuilder.ApplyConfiguration(new ExpenseDetailMap());
            modelBuilder.ApplyConfiguration(new StatusMap());
            modelBuilder.ApplyConfiguration(new AutoApprovePositionMap());
            modelBuilder.ApplyConfiguration(new InvoiceSequenceMap());
        }
    }
}