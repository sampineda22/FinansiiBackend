using System;
using System.Collections.Generic;

#nullable disable

namespace CRM.Models.Payroll
{
    public partial class PlaPersonal
    {
        public string CodEmpresa { get; set; }
        public string CodPersonal { get; set; }
        public string CodAuxiliar { get; set; }
        public string CodAlterno { get; set; }
        public string ApePaterno { get; set; }
        public string ApeMaterno { get; set; }
        public string NomTrabajador { get; set; }
        public DateTime FecNacimiento { get; set; }
        public string TipSexo { get; set; }
        public string TipEstadoCivil { get; set; }
        public decimal? NumHijos { get; set; }
        public byte[] BmpFoto { get; set; }
        public string CodProfesion { get; set; }
        public string TipGradoInstruc { get; set; }
        public DateTime? FecIngreso { get; set; }
        public string CodTipoPlanilla { get; set; }
        public string NumVerCCostos { get; set; }
        public string CodCCostos { get; set; }
        public string IndAdelaQuincenal { get; set; }
        public string TipEstado { get; set; }
        public string TipPago { get; set; }
        public string CodMonedaPago { get; set; }
        public string TipCuentaBancoPago { get; set; }
        public string NumCuentaBancoPago { get; set; }
        public string CodAuxiliarBancoPago { get; set; }
        public string CodCategoria { get; set; }
        public string CodCargo { get; set; }
        public string CodSeguroSocial { get; set; }
        public string CodZona { get; set; }
        public DateTime? FecCesado { get; set; }
        public string DesMotivoCese { get; set; }
        public string IndOficina { get; set; }
        public string IndEventual { get; set; }
        public string IndRatificado { get; set; }
        public DateTime? FecRatificacion { get; set; }
        public DateTime? FecUltCobro { get; set; }
        public decimal? NumItem { get; set; }
        public string CodMoneda { get; set; }
        public decimal? ImpSueldo { get; set; }
        public decimal? NumMesDeduccionIsr { get; set; }
        public decimal? ImpSueldoDiario { get; set; }
        public string CodUserActual { get; set; }
        public DateTime? FecActualiza { get; set; }
        public string DesEmail { get; set; }
        public string TipRelacionLaboral { get; set; }
        public string CodAfp { get; set; }
        public string NumAfiliacionAfp { get; set; }
        public string PrimerNombre { get; set; }
        public string SegundoNombre { get; set; }
        public string CcFinanzas { get; set; }
        public string CuentaContable { get; set; }
        public string NumCuentaBancoPagobd { get; set; }
        public string CodAuxiliarBancoPagobd { get; set; }
        public string NivelSalarial { get; set; }
        public string CategoriaSalarial { get; set; }
        public string PromedioBonos { get; set; }
        public string TipoEmpresa { get; set; }
        public string JefeInmediato { get; set; }
        public string ActiveDirectory { get; set; }
        public int? Turnoid { get; set; }
        public string ImCodCiuimp { get; set; }
        public int? ImCodEtnia { get; set; }
        public string IndPerIncapacidad { get; set; }
        public DateTime? FecPreRetiro { get; set; }
        public string CodPreRetiro { get; set; }
        public string ObsPreRetiro { get; set; }
        public string CodPerIncapacidad { get; set; }
        public DateTime? FecInicioIncapacidad { get; set; }
        public DateTime? FecFinalIncapacidad { get; set; }
        public string ObsPerIncapacidad { get; set; }
        public DateTime? FecPermanencia { get; set; }
    }
}
