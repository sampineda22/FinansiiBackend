using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsModels_PW.Dtos
{
    public class Detalle_BoletaPago
    {
        public string Cod_Empresa { get; set; }
        public string CodPersonal { get; set; }
        public string CodAlterno { get; set; }
        public string NombreEmpleado { get; set; }
        public string CodPlanilla { get; set; }
        public string DesPlanilla { get; set; }
        public string Cod_Centro_Costo { get; set; }
        public string Centro_Costo { get; set; }
        public string Semana_PR { get; set; }
        public string Dia_Inicio { get; set; }
        public string Mes_Inicio { get; set; }
        public string Año_Inicio { get; set; }
        public string Dia_Final { get; set; }
        public string Mes_Final { get; set; }
        public string Año_Final { get; set; }
        public DateTime Fecha_Inicio_Semana { get; set; }
        public DateTime Fecha_Fin_Semana { get; set; }
        //Conceptos - movimientos
        public string Cod_Concepto { get; set; }
        public string Des_Concepto { get; set; }
        public string Tipo_Concepto { get; set; }
        public decimal Valor_Concepto { get; set; }
        public decimal Saldo_Concepto { get; set; }
    }

    public class Encabezado_Detalle
    {
        public string Cod_Empresa { get; set; }
        public string CodPersonal { get; set; }
        public string CodAlterno { get; set; }
        public string NombreEmpleado { get; set; }
        public string Cod_Centro_Costo { get; set; }
        public string Centro_Costo { get; set; }
        //Semana
        public string Semana_PR { get; set; }
        //Supervisor
        public string supervisor { get; set; }
        //Movimientos
        public int Cod_Tipo_Concepto { get; set; }
        public string Tipo_Concepto { get; set; }
        public string Cod_Concepto { get; set; }
        public string Des_Concepto { get; set; }
        public string Valor_Concepto { get; set; }
        public string Saldo_Concepto { get; set; }
        public string Valor_Tiempo { get; set; }
        public string Balance_favor { get; set; }
        public string Sobregiro { get; set; }
        public string neto_Pagar { get; set; }
        //Iterador
        public string noPagina { get; set; }
        //Auxiliares
        public string encabezado1 { get; set; }
        public string encabezado2 { get; set; }
        public string valor_encabezado1 { get; set; }
        public string valor_encabezado2 { get; set; }
        public string Total_Valor_TipoConcepto { get; set; }
        public string Total_Tiempo_TipoConcepto { get; set; }
        public string total_TipoConcepto { get; set; }
    }

    public class Movimientos_Conceptos
    {
        public string CodPersonal { get; set; }
        public string Tipo_Concepto { get; set; }
        public string Cod_Concepto { get; set; }
        public string Des_Concepto { get; set; }
        public decimal Valor_Concepto { get; set; }
        public decimal Saldo_Concepto { get; set; }
        public decimal Valor_Tiempo { get; set; }
    }



    public class Ingresos
    {
        public string Cod_Concepto { get; set; }
        public string Des_Concepto { get; set; }
        public decimal Valor_Concepto { get; set; }
        public decimal Saldo_Concepto { get; set; }
    }

 

}
