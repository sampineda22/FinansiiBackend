using CRM.Features.Gira.ExpensesSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Gira
{
    public class ExpenseAccountMap : IEntityTypeConfiguration<ExpenseAccount>
    {
        public void Configure(EntityTypeBuilder<ExpenseAccount> builder)
        {
            builder.ToTable("ExpenseAccount", "Gira");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired().UseIdentityColumn();
            builder.Property(e => e.AccountId).HasMaxLength(12).IsRequired();
            builder.Property(e => e.IdExpenseType).IsRequired();
            builder.Property(e => e.IdExpenseCategory).IsRequired();
            builder.Property(e => e.CompanyCode).HasMaxLength(4).IsRequired();

            builder.HasOne(e => e.ExpenseType)
                .WithMany(e => e.ExpensesAccounts)
                .HasForeignKey(e => e.IdExpenseType)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(e => e.ExpenseCategory)
                .WithMany(e => e.ExpensesAccounts)
                .HasForeignKey(e => e.IdExpenseCategory)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
