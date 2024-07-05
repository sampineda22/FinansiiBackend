using System;
using System.Collections.Generic;

#nullable disable

namespace CRM.Models.PayWeb
{
    public partial class ControlEmpresa
    {
        public int IdControl { get; set; }
        public string CodEmpresaPayRoll { get; set; }
        public int CodEmpresaIntegrado { get; set; }
        public string CodAx { get; set; }
    }
}
