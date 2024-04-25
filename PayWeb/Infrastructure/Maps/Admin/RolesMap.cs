using CRM.Features.Admin.Roles;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Maps.Admin
{
    public class RolesMap : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles", "Admin");
            builder.HasKey(e => new { e.RoleId, e.CompanyCode });
            builder.Property(e => e.RoleId).IsRequired().HasColumnType("int").UseIdentityColumn();
            builder.Property(e => e.CompanyCode).IsRequired().HasColumnType("varchar").HasMaxLength(4);
            builder.Property(e => e.Description).IsRequired().HasColumnType("varchar").HasMaxLength(60);
            builder.Property(e => e.CreationDate).IsRequired().HasColumnType("datetime");
            builder.Property(e => e.CreationUser).IsRequired().HasColumnType("varchar").HasMaxLength(20);
            builder.Property(e => e.UpdateDate).HasColumnType("datetime");
            builder.Property(e => e.UpdateUser).HasColumnType("varchar").HasMaxLength(20);
        }
    }
}
