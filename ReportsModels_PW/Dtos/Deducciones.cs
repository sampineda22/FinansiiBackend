using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsModels_PW.Dtos
{

    public class CooperativaIntDto
    {
        public string codPersonal { get; set; }
        public string codAlterno { get; set; }
        public string nombreEmpleado { get; set; }
        public string ded_Ahorro { get; set; }
        public string ded_Prestamo { get; set; }
        public string deduccion_Total { get; set; }
        public string saldo_Aportacion { get; set; }
        public string saldo_Prestamo { get; set; }
        public string saldo_Real { get; set; }

        //Totales
        public string T_ded_Ahorro { get; set; }
        public string T_ded_Prestamo { get; set; }
        public string T_deduccion_Total { get; set; }
        public string T_saldo_Aportacion { get; set; }
        public string T_saldo_Prestamo { get; set; }
        public string T_saldo_Real { get; set; }

        //auxiliares
        public string agrupador { get; set; }
        public string Nom_empresa { get; set; }
        public string Fecha_Generacion { get; set; }
        public string socios { get; set; }
        public string No_Semana { get; set; }
        public string Proceso_Aux { get; set; }
        public string Fecha_Aux { get; set; }
    }

    public class CooperativaDto
    {
        public string fechaRegistro { get; set; }
        public string fechaCancelado { get; set; }
        public string codRegistro { get; set; }
        public string planilla { get; set; }
        public string codPersonal { get; set; }
        public string codAlterno { get; set; }
        public string nombreEmpleado { get; set; }
        public string concepto { get; set; }
        public string Saldo { get; set; }
        public string deduccion { get; set; }
        public string cuotaNivelada { get; set; }
        public decimal noPagos { get; set; }
        public string moneda { get; set; }
        public string estado { get; set; }

        //Auxiliares
        public string agrupador { get; set; }
        public string Fecha_Aux{ get; set; }
        public string TotalDeducciones { get; set; }
        public string Proceso_Aux { get; set; }
        public string No_Semana { get; set; }
    }

}
