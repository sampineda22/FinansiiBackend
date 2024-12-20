using CRM.Features.Accounting.BankConfiguration;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using CRM.Features.Accounting.BankStatement;

namespace CRM.Infrastructure.Maps.Accounting
{
    public class TransactionCodeMap : IEntityTypeConfiguration<TransactionCode>
    {
        public void Configure(EntityTypeBuilder<TransactionCode> builder)
        {
            builder.ToTable("TransactionCodes", "Finansii");
            builder.HasKey(e => new { e.BankAccountId, e.Code });
            builder.Property(e => e.Code).HasMaxLength(8).IsRequired();
            builder.Property(e => e.BankAccountId).HasMaxLength(60).IsRequired();
            builder.Property(e => e.Description).HasMaxLength(250).IsRequired();
            builder.Property(e => e.TransactionType).HasMaxLength(2).IsRequired();
        }
    }
}
