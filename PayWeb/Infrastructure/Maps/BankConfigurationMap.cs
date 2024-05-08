using CRM.Features.BankConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Infrastructure.Maps
{
    public class BankConfigurationMap : IEntityTypeConfiguration<BankConfiguration>
    {
        public void Configure(EntityTypeBuilder<BankConfiguration> builder)
        {
            builder.ToTable("BankConfiguration", "Finansii");
            builder.HasKey(e => e.BankConfigurationId);
            builder.Property(e => e.CompanyId).HasMaxLength(4).IsRequired();
            builder.Property(e => e.AccountId).HasMaxLength(20).IsRequired();
            builder.Property(e => e.AccountNumber).HasMaxLength(100).IsRequired();
            builder.Property(e => e.Host).HasMaxLength(150).IsRequired();
            builder.Property(e => e.UserName).HasMaxLength(75).IsRequired();
            builder.Property(e => e.Password).HasMaxLength(75).IsRequired();
            builder.Property(e => e.FileRoute).HasMaxLength(200).IsRequired();
            builder.Property(e => e.LocalFileRoute).HasMaxLength(200).IsRequired();
            builder.Property(e => e.FileName).HasMaxLength(75).IsRequired();
        }
    }
}
