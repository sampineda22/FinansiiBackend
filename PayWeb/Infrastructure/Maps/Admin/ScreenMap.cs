using CRM.Features.Admin.Screen;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Admin
{
    public class ScreenMap : IEntityTypeConfiguration<Screen>
    {
        public void Configure(EntityTypeBuilder<Screen> builder)
        {
            builder.ToTable("Screens", "Finansii");
            builder.HasKey(e => e.Code);
            builder.Property(e => e.Code).HasColumnName("Code")
                                                .HasMaxLength(30);
            builder.Property(e => e.Name).HasColumnName("Name")
                                                .HasMaxLength(100);
            builder.Property(e => e.Route).HasColumnName("Route")
                                                .HasMaxLength(100);
            builder.Property(e => e.ParentCode).HasColumnName("ParentCode")
                                                .HasMaxLength(30);
            builder.Property(e => e.Icon).HasMaxLength(60);
        }
    }
}
