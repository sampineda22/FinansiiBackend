using CRM.Features.Accounting.CD;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps
{
    public class CertificateDepositMap : IEntityTypeConfiguration<CertificateDeposit>
    {
        public void Configure(EntityTypeBuilder<CertificateDeposit> builder)
        {
            builder.ToTable("CertificatesDeposit", "Finansii");
            builder.HasKey(e =>  e.Id);
            builder.Property(e => e.Id).IsRequired().UseIdentityColumn();
            builder.Property(e => e.CompanyCode).HasMaxLength(4).IsRequired();
            builder.Property(e => e.Bank).IsRequired();
            builder.Property(e => e.CDNumber).HasMaxLength(30).IsRequired();
            builder.Property(e => e.Currency).IsRequired();
            builder.Property(e => e.StartDate).HasColumnType("date").IsRequired();
            builder.Property(e => e.EndDate).HasColumnType("date").IsRequired();
            builder.Property(e => e.Amount).HasColumnType("decimal(18, 2)").IsRequired();
            builder.Property(e => e.RatePercentage).HasColumnType("decimal(18, 2)").IsRequired();
            builder.Property(e => e.DailyIncome).HasColumnType("decimal(18, 4)").IsRequired();
            builder.Property(e => e.isEnabled).HasColumnType("bit");
            builder.Property(e => e.RenovationCertificate).HasMaxLength(30);
            builder.Property(e => e.Comment).HasColumnType("varchar(MAX)");
            builder.Property(e => e.isCapitalizable).HasColumnType("bit");
            builder.Property(e => e.CreationDate).HasColumnType("datetime").IsRequired().HasDefaultValueSql("'1900-01-01'");
            builder.Property(e => e.CreationUser).HasMaxLength(30).IsRequired();
            builder.Property(e => e.ModificationDate).HasColumnType("datetime");
            builder.HasMany(c => c.WeeklyRecords).WithOne(c => c.CertificateDeposit).HasForeignKey(c => c.CertificateId)
                                                           .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

        }
    }
}
