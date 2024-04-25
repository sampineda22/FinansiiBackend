using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Common
{
    public class MT940Transaction
    {
        public string Account { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public string CurrencyCode { get; set; }

        public override string ToString()
        {
            return $"Account:{Account}, Date: {Date}, Monto: {Amount}, Type: {Type}, Reference: {Reference}, Descripcion: {Description}";
        }
    }
}
