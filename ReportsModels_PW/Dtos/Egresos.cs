using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsModels_PW.Dtos
{
    public class Egresos
    {
        public string Cod_Concepto { get; set; }
        public string Des_Concepto { get; set; }
        public decimal Valor_Concepto { get; set; }
        public decimal Saldo_Concepto { get; set; }
    }
}
