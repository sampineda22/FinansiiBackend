using System;
using CRM.Models.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace CRM.Infrastructure
{
    public partial class PayrollContext : DbContext
    {
        public PayrollContext()
        {
        }

        public PayrollContext(DbContextOptions<PayrollContext> options)
            : base(options)
        {
        }

        public virtual DbSet<PlaPersonal> PlaPersonals { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=Payroll");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<PlaPersonal>(entity =>
            {
                entity.HasKey(e => new { e.CodEmpresa, e.CodPersonal })
                    .HasName("PK__PLA_PERSONAL__157422BC");

                entity.ToTable("PLA_PERSONAL");

                entity.HasIndex(e => new { e.CodAuxiliar, e.CodEmpresa }, "PLA_PERSONAL_FK_03");

                entity.HasIndex(e => new { e.CodAuxiliarBancoPago, e.CodEmpresa }, "PLA_PERSONAL_FK_04");

                entity.HasIndex(e => e.CodEmpresa, "PLA_PERSONAL_FK_06");

                entity.HasIndex(e => e.CodZona, "PLA_PERSONAL_FK_07");

                entity.HasIndex(e => new { e.CodTipoPlanilla, e.CodEmpresa }, "PLA_PERSONAL_FK_09");

                entity.HasIndex(e => new { e.CodCategoria, e.CodCargo, e.CodEmpresa }, "PLA_PERSONAL_FK_10");

                entity.HasIndex(e => e.CodProfesion, "PLA_PERSONAL_FK_11");

                entity.HasIndex(e => new { e.NumVerCCostos, e.CodCCostos, e.CodEmpresa }, "PLA_PERSONAL_FK_13");

                entity.HasIndex(e => e.CodMonedaPago, "PLA_PERSONAL_FK_14");

                entity.Property(e => e.CodEmpresa)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .HasColumnName("COD_EMPRESA");

                entity.Property(e => e.CodPersonal)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .HasColumnName("COD_PERSONAL");

                entity.Property(e => e.ActiveDirectory)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("ACTIVE_DIRECTORY");

                entity.Property(e => e.ApeMaterno)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("APE_MATERNO");

                entity.Property(e => e.ApePaterno)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("APE_PATERNO");

                entity.Property(e => e.BmpFoto)
                    .HasColumnType("image")
                    .HasColumnName("BMP_FOTO");

                entity.Property(e => e.CategoriaSalarial)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("Categoria_Salarial");

                entity.Property(e => e.CcFinanzas)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("CC_FINANZAS");

                entity.Property(e => e.CodAfp)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .HasColumnName("COD_AFP");

                entity.Property(e => e.CodAlterno)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("COD_ALTERNO");

                entity.Property(e => e.CodAuxiliar)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .HasColumnName("COD_AUXILIAR");

                entity.Property(e => e.CodAuxiliarBancoPago)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .HasColumnName("COD_AUXILIAR_BANCO_PAGO");

                entity.Property(e => e.CodAuxiliarBancoPagobd)
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .HasColumnName("COD_AUXILIAR_BANCO_PAGOBD");

                entity.Property(e => e.CodCCostos)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("COD_C_COSTOS");

                entity.Property(e => e.CodCargo)
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .HasColumnName("COD_CARGO");

                entity.Property(e => e.CodCategoria)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .HasColumnName("COD_CATEGORIA");

                entity.Property(e => e.CodMoneda)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .HasColumnName("COD_MONEDA");

                entity.Property(e => e.CodMonedaPago)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .HasColumnName("COD_MONEDA_PAGO");

                entity.Property(e => e.CodPerIncapacidad)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .HasColumnName("COD_PER_INCAPACIDAD");

                entity.Property(e => e.CodPreRetiro)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .HasColumnName("COD_PRE_RETIRO");

                entity.Property(e => e.CodProfesion)
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .HasColumnName("COD_PROFESION");

                entity.Property(e => e.CodSeguroSocial)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("COD_SEGURO_SOCIAL");

                entity.Property(e => e.CodTipoPlanilla)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .HasColumnName("COD_TIPO_PLANILLA");

                entity.Property(e => e.CodUserActual)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("COD_USER_ACTUAL");

                entity.Property(e => e.CodZona)
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .HasColumnName("COD_ZONA");

                entity.Property(e => e.CuentaContable)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("CUENTA_CONTABLE");

                entity.Property(e => e.DesEmail)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("des_email");

                entity.Property(e => e.DesMotivoCese)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("DES_MOTIVO_CESE");

                entity.Property(e => e.FecActualiza)
                    .HasColumnType("datetime")
                    .HasColumnName("FEC_ACTUALIZA");

                entity.Property(e => e.FecCesado)
                    .HasColumnType("datetime")
                    .HasColumnName("FEC_CESADO");

                entity.Property(e => e.FecFinalIncapacidad)
                    .HasColumnType("datetime")
                    .HasColumnName("FEC_FINAL_INCAPACIDAD");

                entity.Property(e => e.FecIngreso)
                    .HasColumnType("datetime")
                    .HasColumnName("FEC_INGRESO");

                entity.Property(e => e.FecInicioIncapacidad)
                    .HasColumnType("datetime")
                    .HasColumnName("FEC_INICIO_INCAPACIDAD");

                entity.Property(e => e.FecNacimiento)
                    .HasColumnType("datetime")
                    .HasColumnName("FEC_NACIMIENTO");

                entity.Property(e => e.FecPermanencia)
                    .HasColumnType("datetime")
                    .HasColumnName("FEC_PERMANENCIA");

                entity.Property(e => e.FecPreRetiro)
                    .HasColumnType("datetime")
                    .HasColumnName("FEC_PRE_RETIRO");

                entity.Property(e => e.FecRatificacion)
                    .HasColumnType("datetime")
                    .HasColumnName("FEC_RATIFICACION");

                entity.Property(e => e.FecUltCobro)
                    .HasColumnType("datetime")
                    .HasColumnName("FEC_ULT_COBRO");

                entity.Property(e => e.ImCodCiuimp)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .HasColumnName("IM_COD_CIUIMP");

                entity.Property(e => e.ImCodEtnia).HasColumnName("IM_COD_ETNIA");

                entity.Property(e => e.ImpSueldo)
                    .HasColumnType("numeric(12, 2)")
                    .HasColumnName("IMP_SUELDO");

                entity.Property(e => e.ImpSueldoDiario)
                    .HasColumnType("numeric(23, 9)")
                    .HasColumnName("IMP_SUELDO_DIARIO")
                    .HasComputedColumnSql("([IMP_SUELDO] / 30.0000)", false);

                entity.Property(e => e.IndAdelaQuincenal)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("IND_ADELA_QUINCENAL")
                    .IsFixedLength(true);

                entity.Property(e => e.IndEventual)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("IND_EVENTUAL")
                    .IsFixedLength(true);

                entity.Property(e => e.IndOficina)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("IND_OFICINA")
                    .IsFixedLength(true);

                entity.Property(e => e.IndPerIncapacidad)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("IND_PER_INCAPACIDAD")
                    .IsFixedLength(true);

                entity.Property(e => e.IndRatificado)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("IND_RATIFICADO")
                    .IsFixedLength(true);

                entity.Property(e => e.JefeInmediato)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("JEFE_INMEDIATO");

                entity.Property(e => e.NivelSalarial)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("Nivel_Salarial");

                entity.Property(e => e.NomTrabajador)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("NOM_TRABAJADOR");

                entity.Property(e => e.NumAfiliacionAfp)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("NUM_AFILIACION_AFP");

                entity.Property(e => e.NumCuentaBancoPago)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("NUM_CUENTA_BANCO_PAGO");

                entity.Property(e => e.NumCuentaBancoPagobd)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("NUM_CUENTA_BANCO_PAGOBD");

                entity.Property(e => e.NumHijos)
                    .HasColumnType("numeric(2, 0)")
                    .HasColumnName("NUM_HIJOS");

                entity.Property(e => e.NumItem)
                    .HasColumnType("numeric(5, 0)")
                    .HasColumnName("NUM_ITEM");

                entity.Property(e => e.NumMesDeduccionIsr)
                    .HasColumnType("numeric(2, 0)")
                    .HasColumnName("NUM_MES_DEDUCCION_ISR");

                entity.Property(e => e.NumVerCCostos)
                    .HasMaxLength(3)
                    .IsUnicode(false)
                    .HasColumnName("NUM_VER_C_COSTOS");

                entity.Property(e => e.ObsPerIncapacidad)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("OBS_PER_INCAPACIDAD");

                entity.Property(e => e.ObsPreRetiro)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("OBS_PRE_RETIRO");

                entity.Property(e => e.PrimerNombre)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("PRIMER_NOMBRE");

                entity.Property(e => e.PromedioBonos)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("Promedio_Bonos");

                entity.Property(e => e.SegundoNombre)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("SEGUNDO_NOMBRE");

                entity.Property(e => e.TipCuentaBancoPago)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .HasColumnName("TIP_CUENTA_BANCO_PAGO");

                entity.Property(e => e.TipEstado)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .HasColumnName("TIP_ESTADO");

                entity.Property(e => e.TipEstadoCivil)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .HasColumnName("TIP_ESTADO_CIVIL");

                entity.Property(e => e.TipGradoInstruc)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .HasColumnName("TIP_GRADO_INSTRUC");

                entity.Property(e => e.TipPago)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .HasColumnName("TIP_PAGO");

                entity.Property(e => e.TipRelacionLaboral)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .HasColumnName("TIP_RELACION_LABORAL");

                entity.Property(e => e.TipSexo)
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .HasColumnName("TIP_SEXO");

                entity.Property(e => e.TipoEmpresa)
                    .HasMaxLength(30)
                    .IsUnicode(false)
                    .HasColumnName("Tipo_empresa");

                entity.Property(e => e.Turnoid).HasColumnName("TURNOID");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
