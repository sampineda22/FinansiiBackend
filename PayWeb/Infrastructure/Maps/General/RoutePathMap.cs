using CRM.Models.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.General
{
    public class RoutePathMap : IEntityTypeConfiguration<RoutePath>
    {
        public void Configure(EntityTypeBuilder<RoutePath> builder)
        {
            builder.ToTable("RoutePaths", "Finansii");
            builder.HasKey(e => new {e.Id, e.Name});
            builder.Property(e => e.Id).IsRequired().HasColumnType("int").UseIdentityColumn();
            builder.Property(e => e.Name).IsRequired().HasMaxLength(20);
            builder.Property(e => e.URL).IsRequired().HasMaxLength(200);
        }
    }
}
