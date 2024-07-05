using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using CRM.Models.PayWeb;
using CRM.Infrastructure.Maps.Admin;

#nullable disable

namespace CRM.Infrastructure
{
    public partial class PayWebContext : DbContext
    {
        public PayWebContext()
        {
        }

        public PayWebContext(DbContextOptions<PayWebContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ControlEmpresa> ControlEmpresas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=PayWeb");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserMap());

            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<ControlEmpresa>(entity =>
            {
                entity.HasKey(e => e.IdControl);

                entity.Property(e => e.IdControl).HasColumnName("idControl");

                entity.Property(e => e.CodAx)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .HasColumnName("Cod_AX")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.CodEmpresaIntegrado).HasColumnName("Cod_Empresa_Integrado");

                entity.Property(e => e.CodEmpresaPayRoll)
                    .IsRequired()
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .HasColumnName("Cod_Empresa_PayRoll");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
