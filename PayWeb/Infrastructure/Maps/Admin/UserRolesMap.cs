using CRM.Features.Admin.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Admin
{
    public class UserRolesMap : IEntityTypeConfiguration<UserRoles>
    {
        public void Configure(EntityTypeBuilder<UserRoles> builder)
        {
            builder.ToTable("UserRoles", "Finansii");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired().HasColumnType("int").UseIdentityColumn(); ;
            builder.Property(e => e.UserId).IsRequired().HasColumnType("varchar").HasMaxLength(60);
            builder.Property(e => e.RoleCode).IsRequired().HasColumnType("varchar").HasMaxLength(60);
        }
    }
}