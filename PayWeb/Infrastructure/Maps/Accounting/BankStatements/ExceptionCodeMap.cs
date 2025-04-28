using CRM.Features.Accounting.BankStatementServiceAX;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Accounting.BankStatement
{
    public class ExceptionCodeMap : IEntityTypeConfiguration<ExceptionCode>
    {
        public void Configure(EntityTypeBuilder<ExceptionCode> builder)
        {
            builder.ToTable("ExceptionCodes", "Finansii");
            builder.HasKey(e => new { e.AccountId, e.Code });
            builder.Property(e => e.Code).HasMaxLength(25).IsRequired();
            builder.Property(e => e.AccountId).HasMaxLength(20).IsRequired();
            builder.Property(e => e.CompanyCode).HasMaxLength(8).IsRequired();
        }
    }
}
