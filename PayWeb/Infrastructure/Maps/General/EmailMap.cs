using CRM.Models.Finansii;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.General
{
    public class EmailMap : IEntityTypeConfiguration<Email>
    {
        public void Configure(EntityTypeBuilder<Email> builder)
        {
            builder.ToTable("Emails", "Finansii");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.CompanyCode).IsRequired().HasMaxLength(6);
            builder.Property(e => e.PersonalCode).HasMaxLength(20);
            builder.Property(e => e.ProjectCode).IsRequired().HasMaxLength(10);
            builder.Property(e => e.Status).IsRequired();
        }
    }
}
