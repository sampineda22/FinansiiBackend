using CRM.Features.Gira.ExpensesSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Gira
{
    public class ExpenseCategoryMap : IEntityTypeConfiguration<ExpenseCategory>
    {
        public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
        {
            builder.ToTable("ExpenseCategory", "Gira");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired().UseIdentityColumn();
            builder.Property(e => e.IdExpenseType).IsRequired();
            builder.Property(e => e.Name).HasMaxLength(50).IsRequired();
            builder.Property(e => e.IsInvoiceRequired).HasColumnType("bit");
            builder.Property(e => e.IsDescriptionRequired).HasColumnType("bit");
            builder.Property(e => e.IsImageRequired).HasColumnType("bit");
            builder.Property(e => e.Status).HasColumnType("bit");
            builder.Property(e => e.VendAccount).HasMaxLength(50);
            builder.Property(e => e.TaxGroup).HasMaxLength(50);
            builder.Property(e => e.CompanyCode).HasMaxLength(4).IsRequired();

            builder.HasOne(e => e.ExpenseType)
                .WithMany(e => e.ExpensesCategories)
                .HasForeignKey(e => e.IdExpenseType);
        }
    }
}