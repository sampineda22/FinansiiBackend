using CRM.Features.BankStatement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Infrastructure.Maps
{
    public class BankStatementMap : IEntityTypeConfiguration<BankStatement>
    {
        public void Configure(EntityTypeBuilder<BankStatement> builder)
        {
            builder.ToTable("BankStatement");
            builder.HasKey(e => e.BankStatementId);
            builder.Property(e => e.Account).HasMaxLength(20).IsRequired();
            builder.Property(e => e.Description).HasMaxLength(150).IsRequired();
            builder.Property(e => e.CreateDateTime);
        }
    }
}
