namespace CRM.Features.Gira.Historical
{
    public class ExpenseSummaryRow
    {
        public string Description { get; set; } = string.Empty;
        public decimal Lunes { get; set; }
        public decimal Martes { get; set; }
        public decimal Miercoles { get; set; }
        public decimal Jueves { get; set; }
        public decimal Viernes { get; set; }
        public decimal Sabado { get; set; }
    }
}
