using CRM.Features.Gira.ExpensesDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Maps.Gira
{
    public class InvoiceSequenceMap : IEntityTypeConfiguration<InvoiceSequence>
    {
        public void Configure(EntityTypeBuilder<InvoiceSequence> builder)
        {
            builder.ToTable("InvoicesSequences", "Gira");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).IsRequired().UseIdentityColumn();
            builder.Property(e => e.CompanyCode).HasMaxLength(4).IsRequired();
            builder.Property(e => e.Initials).HasMaxLength(6).IsRequired();

            builder.HasIndex(e => new {
                e.CompanyCode,
                e.Initials
            }).IsUnique();
        }
    }
}
