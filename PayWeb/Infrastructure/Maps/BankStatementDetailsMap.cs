using CRM.Features.BankStatementDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Infrastructure.Maps
{
    public class BankStatementDetailsMap : IEntityTypeConfiguration<BankStatementDetails>
    {
        public void Configure(EntityTypeBuilder<BankStatementDetails> builder)
        {
            builder.ToTable("BankStatementDetails");
            builder.HasKey(e => e.BankStatementDetailId);
            builder.Property(e => e.TransactionCode).HasMaxLength(10).IsRequired();
            builder.Property(e => e.Description).HasMaxLength(125).IsRequired();
            builder.Property(e => e.Reference).HasMaxLength(125).IsRequired();
            builder.Property(e => e.TransactionDate).HasColumnType("datetime");
            builder.Property(e => e.Amount).HasPrecision(18, 2);
            builder.Property(e => e.Type);
            builder.HasOne(c => c.BankStatement).WithMany().HasForeignKey(c => c.BankStatementId)
                                                           .HasPrincipalKey(c => c.BankStatementId)
                                                           .OnDelete(deleteBehavior: DeleteBehavior.Restrict);
        }
    }
}
