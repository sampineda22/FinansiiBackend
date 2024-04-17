using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsModels_PW.Dtos
{
    public class Encabezado_BoletaPago
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
        public decimal Total_Ingresos { get; set; }
        public decimal Total_Deducciones { get; set; }
        public decimal Neto { get; set; }
        public decimal HorasTrabajadas { get; set; }
        public decimal HorasExtra { get; set; }
        public decimal TotalHoras { get; set; }
    }
}
