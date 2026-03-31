using CRM.Features.Gira.ExpensesSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Gira
{
    public class ExpenseTypeMap : IEntityTypeConfiguration<ExpenseType>
    {
        public void Configure(EntityTypeBuilder<ExpenseType> builder)
        {
            builder.ToTable("ExpensesType", "Gira");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired().UseIdentityColumn();
            builder.Property(e => e.Name).HasMaxLength(50).IsRequired();
            builder.Property(e => e.Journal).HasMaxLength(50).IsRequired();
            builder.Property(e => e.CompanyCode).HasMaxLength(4).IsRequired();
            builder.Property(e => e.State).HasColumnType("bit");
        }
    }
}
