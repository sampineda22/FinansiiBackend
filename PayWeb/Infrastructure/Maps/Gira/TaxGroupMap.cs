using CRM.Features.Gira.ExpensesSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Gira
{
    public class TaxGroupMap : IEntityTypeConfiguration<TaxGroup>
    {
        public void Configure(EntityTypeBuilder<TaxGroup> builder)
        {
            builder.ToTable("TaxGroup", "Gira");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired().UseIdentityColumn();
            builder.Property(e => e.GrupoImpuestoGravado).HasMaxLength(50);
            builder.Property(e => e.GrupoImpuestoArticuloGravado).HasMaxLength(50);
            builder.Property(e => e.GrupoImpuestoExento).HasMaxLength(50);
            builder.Property(e => e.GrupoImpuestoArticuloExento).HasMaxLength(50);
            builder.Property(e => e.CompanyCode).HasMaxLength(4).IsRequired();
        }
    }
}
