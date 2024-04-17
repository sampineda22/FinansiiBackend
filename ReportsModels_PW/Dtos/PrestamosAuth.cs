using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsModels_PW.Dtos
{
    public class PrestamosAuth
    {
        public string fechaRegistro { get; set; }
        public DateTime fechaCancelado { get; set; }
        public string noPrestamo { get; set; }
        public string codAlterno { get; set; }
        public string nombreEmpleadoInt { get; set; }
        public string codPersonal { get; set; }
        public string nombreEmpleadoPay { get; set; }
        //Variables para desarrollar los calculos
        public decimal ValorPrestamo { get; set; }
        public decimal InteresAnual { get; set; }
        public int Tiempo { get; set; }
        public int NroPagos { get; set; }
        //-----------
        public string moneda { get; set; }
        public string estado { get; set; }
        public string migrado { get; set; }
        public string codRegistro { get; set; }
        public string CIACOD { get; set; }

        //Variables para almacenar los calculos
        public double CuotaNivelada { get; set; }
        public double ValorInteres { get; set; }
        public double TotalPrestamo { get; set; }
    }

    //public class PrestamosAuthReport
    //{
    //    public List<Prestamos_Consolidados> auths { get; set; }
    //}

    public class Prestamos_Consolidados
    {
        public int Ciacod { get; set; }
        public int Cooperativa { get; set; }
        public string Planilla { get; set; }
        public string Cod_Alterno { get; set; }
        public string Cod_Personal { get; set; }
        public string Nombre_Empleado_Int { get; set; }
        public string Nombre_Empleado_PR { get; set; }
        public string Concepto { get; set; }
        public int Canti_Prestamos { get; set; }
        public string Cuota_Manual { get; set; }
        public string Saldo_Restante { get; set; }
        public int No_Pagos { get; set; }
        public int Interes { get; set; }
        public int Tiempo { get; set; }
        public string Fecha_Registro_Saldo { get; set; }
        public int Semana_Registro_Prest { get; set; }
        public string Fecha_P_Prestamo { get; set; }
        public string Fecha_U_Prestamo { get; set; }

        //Others
        public string Cod_Registro { get; set; }
    }


    public class PrestamosDatosRegistro
    {
        public string FechaCreacion { get; set; }
        public string userName { get; set; }
    }
}
