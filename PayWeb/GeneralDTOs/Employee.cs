using System;
using System.Data.SqlTypes;

namespace CRM.GeneralDTOs
{
    public class Employee
    {
       public string PersonalCode { get;set; }
	   public string AlternCode { get; set; }
	   public string Name { get; set; }
       public DateTime StartDate { get; set; }
       public string PayRollTypeCode { get; set; }
       public string CostCenterCode { get; set; }
       public string PositionCode { get; set; }
       public string Position { get; set; }
       public string CategoryCode { get; set; }
       public string Category { get; set; }
    }
}
