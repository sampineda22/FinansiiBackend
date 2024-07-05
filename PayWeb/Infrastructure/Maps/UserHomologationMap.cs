using CRM.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps
{
    public class UserHomologationMap : IEntityTypeConfiguration<UserHomologation>
    {
        public void Configure(EntityTypeBuilder<UserHomologation> builder)
        {
            builder.ToTable("UsersHomologation", "Finansii");
            builder.HasKey(e => new { e.PersonalCode, e.CompanyCode});
            builder.Property(e => e.PersonalCode).HasMaxLength(10).IsRequired();
            builder.Property(e => e.CompanyCode).HasMaxLength(4).IsRequired();
            builder.Property(e => e.AXCode).HasMaxLength(12);
            builder.Property(e => e.CRMCode).HasMaxLength(12);
        }
    }
}
