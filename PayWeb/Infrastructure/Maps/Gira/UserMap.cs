using CRM.Features.Gira.ExpensesSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Gira
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("User", "Gira");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired().UseIdentityColumn();
            builder.Property(e => e.PersonalCode).IsRequired().HasMaxLength(20);
            builder.Property(e => e.Username).IsRequired().HasMaxLength(30);
            builder.Property(e => e.PasswordHash).IsRequired();
            builder.Property(e => e.CompanyCode).HasMaxLength(4).IsRequired();

            builder.HasIndex(e => e.Username).IsUnique();
            builder.HasIndex(e =>e.PersonalCode).IsUnique();
        }
    }
}
