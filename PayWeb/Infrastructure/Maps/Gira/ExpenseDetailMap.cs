using CRM.Features.Gira.Historical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Gira
{
    public class ExpenseDetailMap : IEntityTypeConfiguration<ExpenseDetail>
    {
        public void Configure(EntityTypeBuilder<ExpenseDetail> builder)
        {
            builder.ToTable("ExpenseDetails", "Gira");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired().UseIdentityColumn();
            builder.Property(e => e.ExpenseCategoryId).IsRequired();
            builder.Property(e => e.StatusId).IsRequired();
            builder.Property(e => e.PersonalCode).HasMaxLength(20).IsRequired();
            builder.Property(e => e.VendAccount).HasMaxLength(50);
            builder.Property(e => e.Description).HasMaxLength(250);
            builder.Property(e => e.InvoiceId).HasMaxLength(50).IsRequired();
            builder.Property(e => e.SeriesNum).HasMaxLength(20);
            builder.Property(e => e.InvoiceAmount).IsRequired();
            builder.Property(e => e.InvoiceDate).HasColumnType("datetime").IsRequired().HasDefaultValueSql("'1900-01-01'");
            builder.Property(e => e.ImagePath).HasMaxLength(80);
            builder.Property(e => e.CreationDate).HasColumnType("datetime").IsRequired().HasDefaultValueSql("'1900-01-01'");
            builder.Property(e => e.PersonalCodeAdmin).HasMaxLength(20);
            builder.Property(e => e.RejectionMotive).HasMaxLength(250);
            builder.Property(e => e.JournalNum).HasMaxLength(30);
            builder.Property(e => e.CompanyCode).HasMaxLength(4).IsRequired();

            builder.HasOne(e => e.ExpenseCategory)
                .WithMany(e => e.ExpenseDetails)
                .HasForeignKey(e => e.ExpenseCategoryId);

            builder.HasOne(e => e.FuelType)
                .WithMany(e => e.ExpenseDetails)
                .HasForeignKey(e => e.FuelTypeId);

            builder.HasOne(e => e.Status)
                .WithMany(e => e.ExpenseDetails)
                .HasForeignKey(e => e.StatusId);
        }
    }
}
