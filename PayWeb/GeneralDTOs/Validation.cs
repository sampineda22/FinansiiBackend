namespace CRM.GeneralDTOs
{
    public class Validation
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public string ConditionType { get; set; }
        public string ConditionField {  get; set; }
        public string? ConditionValue { get; set; }
        public string? RequiredType { get; set; }
        public string? RequiredField { get; set; }
        public string? RequiredValue { get; set; }
        public string ProjectCode { get; set; }
    }
}
