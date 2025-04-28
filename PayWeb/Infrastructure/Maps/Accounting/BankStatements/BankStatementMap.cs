using CRM.Features.Accounting.BankStatement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Accounting.BankStatements
{
    public class BankStatementMap : IEntityTypeConfiguration<CRM.Features.Accounting.BankStatement.BankStatement>
    {
        public void Configure(EntityTypeBuilder<CRM.Features.Accounting.BankStatement.BankStatement> builder)
        {
            builder.ToTable("BankStatement", "Finansii");
            builder.HasKey(e => e.BankStatementId);
            builder.Property(e => e.CompanyId).HasMaxLength(4);
            builder.Property(e => e.AccountId).HasMaxLength(20);
            builder.Property(e => e.Account).HasMaxLength(20).IsRequired();
            builder.Property(e => e.TransactionDate).IsRequired();
            builder.Property(e => e.CreateDateTime);
        }
    }
}
