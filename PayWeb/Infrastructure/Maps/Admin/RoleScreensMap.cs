using CRM.Features.Admin.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Admin
{
    public class RoleScreensMap : IEntityTypeConfiguration<RoleScreens>
    {
        public void Configure(EntityTypeBuilder<RoleScreens> builder)
        {
            builder.ToTable("RoleScreens", "Finansii");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired().HasColumnType("int").UseIdentityColumn(); ;
            builder.Property(e => e.RoleCode).IsRequired().HasColumnType("varchar").HasMaxLength(60);
            builder.Property(e => e.ScreenCode).IsRequired().HasColumnType("varchar").HasMaxLength(60);
        }
    }
}