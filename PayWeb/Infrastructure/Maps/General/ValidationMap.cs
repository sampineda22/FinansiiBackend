using CRM.GeneralDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.General
{
    public class ValidationMap : IEntityTypeConfiguration<Validation>
    {
        public void Configure(EntityTypeBuilder<Validation> builder)
        {
            builder.ToTable("Validations", "Finansii");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired().HasColumnType("int").UseIdentityColumn();
            builder.Property(e => e.CompanyCode).IsRequired().HasMaxLength(4);
            builder.Property(e => e.ConditionField).IsRequired().HasMaxLength(30);
            builder.Property(e => e.ConditionValue).HasMaxLength(80);
            builder.Property(e => e.RequiredField).HasMaxLength(30);
            builder.Property(e => e.RequiredValue).HasMaxLength(80);
            builder.Property(e => e.ProjectCode).IsRequired().HasMaxLength(6);
        }
    }
}
