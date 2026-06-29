using CRM.Features.Gira.Approve;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Gira
{
    public class AutoApprovePositionMap : IEntityTypeConfiguration<AutoApprovePosition>
    {
        public void Configure(EntityTypeBuilder<AutoApprovePosition> builder)
        {
            builder.ToTable("AutoApprovePositions", "Gira");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired().UseIdentityColumn();
            builder.Property(e => e.CompanyCode).HasMaxLength(4).IsRequired();
            builder.Property(e => e.CategoryCode).HasMaxLength(4).IsRequired();
            builder.Property(e => e.PositionCode).HasMaxLength(4).IsRequired();

            builder.HasIndex(e => new{
                e.CompanyCode,
                e.CategoryCode,
                e.PositionCode
            }).IsUnique();
        }
    }
}