using CRM.Features.Accounting.AccountingConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Accounting.VendPaymentReport
{
    public class PaymentDateMap : IEntityTypeConfiguration<PaymentDate>
    {
        public void Configure(EntityTypeBuilder<PaymentDate> builder)
        {
            builder.ToTable("PaymentDates", "Finansii");
            builder.HasKey(e => new { e.Month, e.Year, e.CompanyCode});
            builder.Property(e => e.Year).ValueGeneratedNever();
            builder.Property(e => e.Month).ValueGeneratedNever();
            builder.Property(e => e.StartDate).IsRequired();
            builder.Property(e => e.EndDate).IsRequired();
            builder.Property(e => e.CompanyCode).HasMaxLength(4).IsRequired();
        }
    }
}