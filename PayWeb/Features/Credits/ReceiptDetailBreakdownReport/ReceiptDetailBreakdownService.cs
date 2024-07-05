using CRM.Features.BankStatementDetails;
using CRM.Features.Credits.ReceiptBreakdownReport;
using CRM.Infrastructure.Core;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System.Threading.Tasks;
using System;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using CRM.Models.PayWeb;
using System.Linq;
using CRM.Models.Payroll;
using System.Collections.Generic;
using System.IO;
using CRM.Features.Credits.WorkpaperReport;
using Microsoft.Data.SqlClient;
using CRM.Models.General;
using CRM.Features.Credits.ReceiptDetailBreakdownReport;
using System.Drawing;
using CRM.GeneralDTOs;
using Microsoft.AspNetCore.Http;

namespace CRM.Features.Credits.ReceiptBreakdown
{
    public class ReceiptDetailBreakdownService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUnitOfWorkPayWeb _unitOfWorkPayWeb;
        private readonly IUnitOfWorkPayroll _unitOfWorkPayroll;
        private readonly WorkpaperReportService _workpaperReportService;

        public ReceiptDetailBreakdownService(IUnitOfWork unitOfWork, IUnitOfWorkPayWeb unitOfWorkPayWeb, IUnitOfWorkPayroll unitOfWorkPayroll, WorkpaperReportService workpaperReportService)
        {
            _unitOfWork = unitOfWork;
            _unitOfWorkPayWeb = unitOfWorkPayWeb;
            _unitOfWorkPayroll = unitOfWorkPayroll;
            _workpaperReportService = workpaperReportService;
        }

        public async Task<EntityResponse> CreateReceiptDetailBreakdownReports(string start, string end, string weekNumber, string companyCode, string salesAgentSelected = "")
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                EntityResponse response = new();
                SqlParameter[] parameters = { };
                ExcelTable table;
                DateTime startDate = DateTime.Parse(start);
                DateTime endDate = DateTime.Parse(end);
                List<SalesAgent> salesAgents = new();

                int spaceBetweenTables = 5;
                int spaceBetweenSignature = 2;

                string folderPath = "",
                       excelPath = "",
                       week = "",
                       templatePath = _unitOfWork.Repository<RoutePath>().Query().Where(x => x.Name == "RBTemplate").FirstOrDefault().URL,
                       serverPath = _unitOfWork.Repository<RoutePath>().Query().Where(x => x.Name == "Fact").FirstOrDefault().URL;

                week = $"{startDate.ToString("dd-MM-yyyy")} al {endDate.ToString("dd-MM-yyyy")}";

                parameters = new SqlParameter[]
                {
                     new SqlParameter("@EmpresaId", companyCode),
                };
                List<SalesAgent> allSalesAgents = _unitOfWork.Repository<SalesAgent>().GetSP<SalesAgent>("[Finansii].[GetSalesAgents]", parameters, 250).ToList();

                if (salesAgentSelected != "")
                {
                    salesAgents = allSalesAgents.FindAll(x => x.PersonalCode == salesAgentSelected);
                }
                else
                {
                    salesAgents = allSalesAgents;
                }

                foreach (SalesAgent agent in salesAgents)
                {
                    int advanceTableRow = 0;
                    int tableRow = 10;

                    response = _workpaperReportService.GetReportsFolderPath(companyCode, startDate, serverPath, "Desglose de Recibos", agent.Name, week, weekNumber).Result;
                    if (response is EntityResponse<string> genericResponse2)
                    {
                        folderPath = genericResponse2.Data;
                    }
                    else
                    {
                        return EntityResponse.CreateError($"{response.Mensaje}");
                    }

                    response = _workpaperReportService.CopyExcelBook(templatePath, folderPath, "Desglose de Recibos", "Reporte").Result;
                    if (response is EntityResponse<string> genericResponse1)
                    {
                        excelPath = genericResponse1.Data;
                    }
                    else
                    {
                        return EntityResponse.CreateError($"{response.Mensaje}");
                    }

                    FileInfo sourceFile = new(excelPath);

                    using (ExcelPackage package = new(sourceFile))
                    {
                        ExcelWorksheet targetWorksheet = package.Workbook.Worksheets[0];

                        parameters = new SqlParameter[] { };
                        List<Companies> companies = _unitOfWork.Repository<Companies>().GetSP<Companies>("[Finansii].[GetCompaniesNames]", parameters).ToList();

                        parameters = new SqlParameter[]
                        {
                            new SqlParameter("@StartDate", startDate),
                            new SqlParameter("@EndDate", endDate),
                            new SqlParameter("@PersonalCode", agent.PersonalCode),
                            new SqlParameter("@DataAreaId", companyCode),
                        };
                        List<ReceiptDetailBreakdown> receiptDetailBreakdown = _unitOfWork.Repository<ReceiptDetailBreakdown>().GetSP<ReceiptDetailBreakdown>("[Finansii].[ReceiptDetailBreakdown]", parameters).ToList();

                        targetWorksheet.Cells[$"C2"].Value = companies.Find(x => x.CompanyCode == companyCode).Name;
                        targetWorksheet.Cells[$"C4"].Value = startDate.Year.ToString(); //Año
                        targetWorksheet.Cells[$"F4"].Value = agent.Name; //Asesor
                        targetWorksheet.Cells[$"C5"].Value = weekNumber; //Semana
                        targetWorksheet.Cells[$"C6"].Value = week; //Fecha

                        table = targetWorksheet.Tables["TablaDesglose"];

                        foreach (ReceiptDetailBreakdown detail in receiptDetailBreakdown)
                        {
                            targetWorksheet.Cells[$"B{tableRow}"].Value = detail.ReceiptNumber;
                            targetWorksheet.Cells[$"C{tableRow}"].Value = detail.DocumentNumber;
                            targetWorksheet.Cells[$"D{tableRow}"].Value = detail.FELDocument;
                            targetWorksheet.Cells[$"E{tableRow}"].Value = detail.ProductType;
                            targetWorksheet.Cells[$"F{tableRow}"].Value = detail.Date;
                            targetWorksheet.Cells[$"G{tableRow}"].Value = detail.State;
                            targetWorksheet.Cells[$"H{tableRow}"].Value = detail.ClientAccount;
                            targetWorksheet.Cells[$"I{tableRow}"].Value = detail.ClientName;
                            targetWorksheet.Cells[$"J{tableRow}"].Value = detail.DebitCollectorCode;
                            targetWorksheet.Cells[$"K{tableRow}"].Value = detail.CurrencyCode;
                            targetWorksheet.Cells[$"L{tableRow}"].Value = detail.ReceiptAmountInCurrency;
                            targetWorksheet.Cells[$"M{tableRow}"].Value = detail.ReceiptAmount;
                            targetWorksheet.Cells[$"N{tableRow}"].Value = detail.CanceledReceiptAmount;
                            targetWorksheet.Cells[$"O{tableRow}"].Value = detail.CashAmount;
                            targetWorksheet.Cells[$"P{tableRow}"].Value = detail.Total;

                            tableRow++;
                            targetWorksheet.InsertRow(tableRow, 1);
                        }
                        targetWorksheet.DeleteRow(tableRow, 1);

                        var newAddress = new ExcelAddressBase(table.Address.Start.Row, table.Address.Start.Column, tableRow, table.Address.End.Column);
                        typeof(ExcelTable).GetProperty("Address").SetValue(table, newAddress);

                        advanceTableRow = tableRow + spaceBetweenTables;

                        parameters = new SqlParameter[]
                        {
                            new SqlParameter("@SalesAgent", agent.PersonalCode),
                            new SqlParameter("@StartDate", startDate),
                            new SqlParameter("@EndDate", endDate),
                            new SqlParameter("@DataAreaId", companyCode)
                        };
                        List<AppliedAdvance> appliedAdvances = _unitOfWork.Repository<AppliedAdvance>().GetSP<AppliedAdvance>("[Finansii].[GetAdvancesWithInvoices]", parameters).ToList();

                        foreach (AppliedAdvance appliedAdvance in appliedAdvances)
                        {
                            targetWorksheet.Cells[$"B{advanceTableRow}"].Value = appliedAdvance.AdvanceReceipt;
                            targetWorksheet.Cells[$"C{advanceTableRow}"].Value = appliedAdvance.AppliedAdvanceAmount;
                            targetWorksheet.Cells[$"D{advanceTableRow}"].Value = appliedAdvance.Invoice;

                            advanceTableRow++;
                            targetWorksheet.InsertRow(advanceTableRow, 1);
                        }

                        targetWorksheet.DeleteRow(advanceTableRow, 1);

                        table = targetWorksheet.Tables["TablaAnticipos"];
                        newAddress = new ExcelAddressBase(table.Address.Start.Row, table.Address.Start.Column, advanceTableRow, table.Address.End.Column);
                        typeof(ExcelTable).GetProperty("Address").SetValue(table, newAddress);

                        var receipts = receiptDetailBreakdown.Select(e => e.ReceiptNumber).Distinct();
                        string documentsNum = string.Join(",", receipts);

                        parameters = new SqlParameter[]
                        {
                            new SqlParameter("@DocumentsNum", documentsNum),
                            new SqlParameter("@DataAreaId", companyCode)
                        };
                        List<JournalLine> journalLines = _unitOfWork.Repository<JournalLine>().GetSP<JournalLine>("[Finansii].[GetJournalsByDocumentNum]", parameters).ToList();

                        targetWorksheet.Cells[$"C{advanceTableRow + spaceBetweenSignature}"].Value = journalLines.Count <= 0 ? "" : string.Join(", ", journalLines.Select(x => x.ModifiedBy).Distinct());

                        package.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError($"Error en metodo CreateReceiptBreakdownReports: {ex.Message}.");
            }

            return EntityResponse.CreateOk("Reporte generado correctamente");
        }

        public async Task<EntityResponse> GetReceiptDetailBreakdown(string start, string end, string salesmanCode, string companyCode)
        {
            EntityResponse response = new();
            SqlParameter[] parameters = { };
            List<ReceiptDetailBreakdown> receiptDetailBreakdown = new();

            try
            {
                DateTime startDate = DateTime.Parse(start);
                DateTime endDate = DateTime.Parse(end);

                parameters = new SqlParameter[]
                {
                   new SqlParameter("@StartDate", startDate),
                   new SqlParameter("@EndDate", endDate),
                   new SqlParameter("@PersonalCode", salesmanCode),
                   new SqlParameter("@DataAreaId", companyCode),
                };
                receiptDetailBreakdown = _unitOfWork.Repository<ReceiptDetailBreakdown>().GetSP<ReceiptDetailBreakdown>("[Finansii].[ReceiptDetailBreakdown]", parameters, commandTimeout: 180).ToList();

            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError($"Error en metodo GetReceiptDetailBreakdown: {ex.Message}.");
            }

            return EntityResponse.CreateOk(receiptDetailBreakdown);
        }

        public async Task<EntityResponse> GetSalesAgents(string companyCode)
        {
            SqlParameter[] parameters = { };
            List<SalesAgent> salesAgents = new();

            try
            {
                parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmpresaId", companyCode),
                };

                salesAgents = _unitOfWork.Repository<SalesAgent>().GetSP<SalesAgent>("[Finansii].[GetSalesAgents]", parameters).ToList();

            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError($"Error en metodo GetSalesAgents: {ex.Message}.");
            }

            return EntityResponse.CreateOk(salesAgents);
        }

        public async Task<EntityResponse> GetFiscalYears()
        {
            List<FiscalYear> fiscalYears = new();
            try
            {
                SqlParameter[] parameters = {};
                fiscalYears = _unitOfWork.Repository<FiscalYear>().GetSP<FiscalYear>("[Finansii].[GetFiscalYears]", parameters).ToList();

            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError($"Error en metodo GetFiscalYears: {ex.Message}.");
            }

            return EntityResponse.CreateOk(fiscalYears);
        }

        public async Task<EntityResponse> GetFiscalWeeks(string RecId)
        {
            List<FiscalWeek> fiscalWeeks = new();
            try
            {
                SqlParameter[] parameters = 
                {
                    new SqlParameter("@FiscalCalendarYear", RecId)
                };

                fiscalWeeks = _unitOfWork.Repository<FiscalWeek>().GetSP<FiscalWeek>("[Finansii].[GetFiscalWeeks]", parameters).ToList();

            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError($"Error en metodo GetFiscalWeeks: {ex.Message}.");
            }

            return EntityResponse.CreateOk(fiscalWeeks);
        }

    }
}