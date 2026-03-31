namespace CRM.Features.Gira.ExpensesSettings
{
    public class TaxGroup
    {
        public int Id { get;set; }
        public string GrupoImpuestoGravado { get; set; }
        public string GrupoImpuestoArticuloGravado { get; set; }
        public string GrupoImpuestoExento {  get; set; }
        public string GrupoImpuestoArticuloExento { get; set; }
        public string CompanyCode { get; set; }
    }
}
