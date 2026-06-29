using CRM.Features.Gira.ExpensesSettings;
using System;

namespace CRM.Features.Gira.Historical
{
    public class ExpenseDetail
    {
        public int Id { get; set; }
        public int ExpenseCategoryId { get; set; }
        public int? MealId { get; set; }
        public int? FuelTypeId { get; set; }
        public int StatusId { get; set; }
        public string PersonalCode {  get; set; }
        public string? VendAccount { get; set; }        
        public string? Description { get; set; }
        public string? InvoiceId { get; set; }
        public string? SeriesNum { get; set; }
        public double? ExemptAmount { get; set; }
        public double? GravadoAmount { get; set; }
        public double InvoiceAmount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string? ImagePath { get; set; }
        public DateTime CreationDate { get; set; }
        public string? PersonalCodeAdmin {  get; set; }
        public string? RejectionMotive { get; set; }
        public string? JournalNum { get; set; }
        public string CompanyCode { get; set; }
        public string? AXMessage { get;set; }
        public bool InUse { get; set; }
        public virtual ExpenseCategory ExpenseCategory { get; set; }
        public virtual FuelType FuelType { get; set; }
        public virtual Status Status { get; set; }
    }

    public class ExpenseDetailDto
    {
        public int Id { get; set; }
        public int ExpenseCategoryId { get; set; }
        public int? MealId { get; set; }
        public int? FuelTypeId { get; set; }
        public int StatusId { get; set; }
        public string PersonalCode { get; set; }
        public string? VendAccount { get; set; }
        public string? VendName { get; set; }
        public string? VatNum { get; set; }
        public string? Currency { get; set; }
        public string? Description { get; set; }
        public string InvoiceId { get; set; }
        public string? SeriesNum { get; set; }
        public double? ExemptAmount { get; set; }
        public double? GravadoAmount { get; set; }
        public double InvoiceAmount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string? ImagePath { get; set; }
        public DateTime CreationDate { get; set; }
        public string? PersonalCodeAdmin { get; set; }
        public string? RejectionMotive { get; set; }
        public string? JournalNum { get; set; }
        public string CompanyCode { get; set; }
        public string? AXMessage { get; set; }
        public bool InUse { get; set; }
        public string? ExpenseCategoryName { get; set; }
        public string? ExpenseTypeName { get; set; }
        public string? Icon { get; set; }
        public string? FuelTypeName { get; set; }
        public string? StatusName { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
    }
}