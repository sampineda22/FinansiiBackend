using CRM.Features.Gira.Historical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Gira
{
    public class FuelTypeMap : IEntityTypeConfiguration<FuelType>
    {
        public void Configure(EntityTypeBuilder<FuelType> builder)
        {
            builder.ToTable("FuelTypes", "Gira");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired().UseIdentityColumn();
            builder.Property(e => e.Name).HasMaxLength(50).IsRequired();
            builder.Property(e => e.MarkupCode).HasMaxLength(50).IsRequired();
            builder.Property(e => e.CompanyCode).HasMaxLength(4).IsRequired();
        }
    }
}
