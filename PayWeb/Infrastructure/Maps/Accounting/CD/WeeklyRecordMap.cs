using CRM.Features.Accounting.CD;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Accounting.CD
{
    public class WeeklyRecordMap : IEntityTypeConfiguration<WeeklyRecord>
    {
        public void Configure(EntityTypeBuilder<WeeklyRecord> builder)
        {
            builder.ToTable("WeeklyRecords", "Finansii");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired().UseIdentityColumn();
            builder.Property(e => e.CertificateId).IsRequired();
            builder.Property(e => e.AmountInCurrency).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(e => e.Week).IsRequired();
            builder.Property(e => e.Journal).HasMaxLength(30).IsRequired();
            builder.HasOne(c => c.CertificateDeposit).WithMany(c => c.WeeklyRecords).HasForeignKey(c => c.CertificateId);
        }
    }
}
