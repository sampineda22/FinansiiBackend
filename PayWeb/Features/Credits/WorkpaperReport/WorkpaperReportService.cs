using Aspose.Cells;
using CRM.Features.Credits.ReceiptDetailBreakdownReport;
using CRM.Features.Credits.WorkpaperReport;
using CRM.Infrastructure.Core;
using CRM.Models.General;
using CRM.Models.PayWeb;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Features.Credits.ReceiptBreakdownReport
{
    public class WorkpaperReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUnitOfWorkPayWeb _unitOfWorkPayWeb;
        private readonly IUnitOfWorkPayroll _unitOfWorkPayroll;

        public WorkpaperReportService(IUnitOfWork unitOfWork, IUnitOfWorkPayWeb unitOfWorkPayWeb, IUnitOfWorkPayroll unitOfWorkPayroll)
        {
            _unitOfWork = unitOfWork;
            _unitOfWorkPayWeb = unitOfWorkPayWeb;
            _unitOfWorkPayroll = unitOfWorkPayroll;
        }

        public async Task<EntityResponse> CreateWorkpaperReports(string start, string end, string weekNumber, string companyCode, string salesAgentSelected = "")
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                List<SalesAgent> salesAgents = new();
                SqlParameter[] parameters = { };
                EntityResponse response = new();
                ExcelTable table;

                DateTime startDate = DateTime.Parse(start);
                DateTime endDate = DateTime.Parse(end);

                string folderPath = "",
                       excelPath = "",
                       week = "",
                       templatePath       = _unitOfWork.Repository<RoutePath>().Query().Where(x => x.Name == "WPTemplate").FirstOrDefault().URL,
                       serverPath         = _unitOfWork.Repository<RoutePath>().Query().Where(x => x.Name == "Fact").FirstOrDefault().URL,
                       companyCodePayRoll = _unitOfWorkPayWeb.Repository<ControlEmpresa>().Query().Where(x => x.CodAx == companyCode).FirstOrDefault().CodEmpresaPayRoll;
                int spaceBetweenTables = 5;

                week = $"{startDate.ToString("dd-MM-yyyy")} al {endDate.ToString("dd-MM-yyyy")}";

                parameters = new SqlParameter[]
                {
                     new SqlParameter("@EmpresaId", companyCode),
                };
                List<SalesAgent> allSalesAgents = _unitOfWork.Repository<SalesAgent>().GetSP<SalesAgent>("[Finansii].[GetSalesAgents]", parameters).ToList();

                if (salesAgentSelected != "x")
                {
                    salesAgents = allSalesAgents.FindAll(x => x.PersonalCode == salesAgentSelected);
                }
                else
                {
                    salesAgents = allSalesAgents;
                }

                parameters = new SqlParameter[] { };
                List<Companies> companies = _unitOfWork.Repository<Companies>().GetSP<Companies>("[Finansii].[GetCompaniesNames]", parameters).ToList();

                foreach (SalesAgent salesAgent in salesAgents)
                {
                    int tableRow = 10;
                    int summaryTableRow = 0;

                    parameters = new SqlParameter[]
                    {
                        new SqlParameter("@StartDate", startDate),
                        new SqlParameter("@EndDate", endDate),
                        new SqlParameter("@PersonalCode", salesAgent.PersonalCode),
                        new SqlParameter("@DataAreaId", companyCode),
                    };
                    List<WorkPaper> workPaperDetails = _unitOfWork.Repository<WorkPaper>().GetSP<WorkPaper>("[Finansii].[WorkpaperReport]", parameters).ToList();

                    if(workPaperDetails.Count > 0)
                    {
                        response = GetReportsFolderPath(companyCode, startDate, serverPath, "Cédulas de Asesores de Venta", salesAgent.Name, week, weekNumber).Result;

                        if (response is EntityResponse<string> genericResponse2)
                        {
                            folderPath = genericResponse2.Data;
                        }
                        else
                        {
                            return EntityResponse.CreateError($"{response.Mensaje}");
                        }

                        response = CopyExcelBook(templatePath, folderPath, "Reporte de Cédula -" + salesAgent.PersonalCode, "Reporte").Result;
                        string pdfFilePath = $@"{folderPath}\{"Reporte de Cédula -" + salesAgent.PersonalCode}.pdf";

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
                            string vouchersJoined = "", receipt = "";

                            targetWorksheet.Cells[$"C2"].Value = companies.Find(x => x.CompanyCode.ToLower() == companyCode.ToLower()).Name;
                            targetWorksheet.Cells[$"C4"].Value = startDate.Year.ToString(); //Año
                            targetWorksheet.Cells[$"F4"].Value = salesAgent.Name; //Asesor
                            targetWorksheet.Cells[$"C5"].Value = weekNumber; //Semana
                            targetWorksheet.Cells[$"C6"].Value = week; //Fecha


                            table = targetWorksheet.Tables["TablaCedula"];

                            foreach (WorkPaper detail in workPaperDetails)
                            {
                                targetWorksheet.Cells[$"B{tableRow}"].Value = receipt == detail.ReceiptNumber ? "" : detail.ReceiptNumber;
                                targetWorksheet.Cells[$"C{tableRow}"].Value = detail.ProcessDate;
                                targetWorksheet.Cells[$"D{tableRow}"].Value = detail.State;
                                targetWorksheet.Cells[$"E{tableRow}"].Value = detail.Workpaper;
                                targetWorksheet.Cells[$"F{tableRow}"].Value = detail.ClientAccount;
                                targetWorksheet.Cells[$"G{tableRow}"].Value = detail.ClientName;
                                targetWorksheet.Cells[$"H{tableRow}"].Value = detail.DebtCollector;
                                targetWorksheet.Cells[$"I{tableRow}"].Value = detail.CurrencyCode;
                                targetWorksheet.Cells[$"J{tableRow}"].Value = detail.ReceiptAmountInCurrency;
                                targetWorksheet.Cells[$"K{tableRow}"].Value = detail.ReceiptAmount;
                                targetWorksheet.Cells[$"L{tableRow}"].Value = detail.PaymentMethod;
                                targetWorksheet.Cells[$"M{tableRow}"].Value = detail.CashAmount;
                                targetWorksheet.Cells[$"N{tableRow}"].Value = detail.TransferAmount;
                                targetWorksheet.Cells[$"O{tableRow}"].Value = detail.DeductedAmount;
                                targetWorksheet.Cells[$"P{tableRow}"].Value = detail.CheckAmount;
                                targetWorksheet.Cells[$"Q{tableRow}"].Value = detail.PostdatedCheckAmount;
                                targetWorksheet.Cells[$"R{tableRow}"].Value = detail.CheckDueDate == null || detail.CheckDueDate == "" ? "" : Convert.ToDateTime(detail.CheckDueDate);
                                targetWorksheet.Cells[$"S{tableRow}"].Value = detail.BankName;

                                targetWorksheet = ApplyBorderToReport(tableRow, targetWorksheet, 'S', receipt != detail.ReceiptNumber);

                                if (receipt != detail.ReceiptNumber)
                                {
                                    receipt = detail.ReceiptNumber;
                                }
                                else
                                {
                                    targetWorksheet.Cells[$"B{tableRow}"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.None;
                                }

                                tableRow++;
                                targetWorksheet.InsertRow(tableRow, 1);
                            }

                            targetWorksheet.DeleteRow(tableRow, 1);

                            var newAddress = new ExcelAddressBase(table.Address.Start.Row, table.Address.Start.Column, tableRow, table.Address.End.Column);
                            typeof(ExcelTable).GetProperty("Address").SetValue(table, newAddress);

                            summaryTableRow = tableRow + spaceBetweenTables;

                            vouchersJoined = string.Join(",", workPaperDetails.Select(e => e.Voucher.ToString()));

                            parameters = new SqlParameter[]
                            {
                            new SqlParameter("@Vouchers", vouchersJoined),
                            new SqlParameter("@DataAreaId", companyCode)
                            };
                            List<WorkPaperSummary> workPaperSummaryDetail = _unitOfWork.Repository<WorkPaperSummary>().GetSP<WorkPaperSummary>("[Finansii].[WorkpaperReportSummary]", parameters).ToList();

                            foreach (WorkPaperSummary detail in workPaperSummaryDetail)
                            {
                                targetWorksheet.Cells[$"B{summaryTableRow}"].Value = detail.Account;
                                targetWorksheet.Cells[$"C{summaryTableRow}"].Value = detail.AccountName;
                                targetWorksheet.Cells[$"D{summaryTableRow}"].Value = detail.Credit;
                                targetWorksheet.Cells[$"D{summaryTableRow}"].Style.Numberformat.Format = "#,##0.00";
                                targetWorksheet.Cells[$"E{summaryTableRow}"].Value = detail.Debit;
                                targetWorksheet.Cells[$"E{summaryTableRow}"].Style.Numberformat.Format = "#,##0.00";

                                summaryTableRow++;
                                targetWorksheet.InsertRow(summaryTableRow, 1);
                            }

                            targetWorksheet.DeleteRow(summaryTableRow, 1);

                            table = targetWorksheet.Tables["TablaResumen"];
                            newAddress = new ExcelAddressBase(table.Address.Start.Row, table.Address.Start.Column, summaryTableRow, table.Address.End.Column);
                            typeof(ExcelTable).GetProperty("Address").SetValue(table, newAddress);

                            package.Save();
                        }

                        ConvertExcelToPdf(excelPath, pdfFilePath);
                        File.Delete(excelPath);
                    }
                }
           }
            catch (Exception ex)
            {
                return EntityResponse.CreateError($"Error en metodo CreateWorkpaperReports: {ex.Message}.");
            }

            return EntityResponse.CreateOk();
        }

        public ExcelWorksheet ApplyBorderToReport(int rowNumber, ExcelWorksheet worksheet, char lastChar, bool applyTopBorder)
        {
            try
            {
                for (char c = 'B'; c <= lastChar; c++)
                {
                    worksheet.Cells[$"{c}{rowNumber}"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"{c}{rowNumber}"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"{c}{rowNumber}"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheet.Cells[$"{c}{rowNumber}"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                worksheet.Cells[$"B{rowNumber}"].Style.Border.Top.Style = applyTopBorder ? OfficeOpenXml.Style.ExcelBorderStyle.Thin : 0;
            }
            catch (Exception ex) { 
                Console.WriteLine(ex.Message);
            }

            return worksheet;
        }

        public void ConvertExcelToPdf(string excelFilePath, string pdfFilePath)
        {
            try
            {
                Workbook workbook = new Workbook(excelFilePath);
                Worksheet worksheet = workbook.Worksheets[0];

                PdfSaveOptions options = new PdfSaveOptions();

                options.AllColumnsInOnePagePerSheet = true;
                options.CalculateFormula = true;

                workbook.Save(pdfFilePath, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public async Task<EntityResponse> GetAccountingWeek(DateTime date)
        {
            AccountingWeek accountingWeek = new();

            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@Date",date)
                };

                accountingWeek = _unitOfWork.Repository<AccountingWeek>().GetSP<AccountingWeek>("[Finansii].[GetAccountingWeek]", parameters).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError($"{ex.Message}.");
            }

            return EntityResponse.CreateOk(accountingWeek);
        }

        public async Task<EntityResponse> GetReportsFolderPath(string companyCode, DateTime date, string folderPath, string folderName, string salesAgentName, string week, string weekNumber)
        {
            EntityResponse response = new();       

            try
            {
                SqlParameter[] parameters = {};

                List<Companies> companies = _unitOfWork.Repository<Companies>().GetSP<Companies>("[Finansii].[GetCompaniesNames]", parameters).ToList();
                folderPath += $@"{companies.Find(x => x.CompanyCode.ToLower() == companyCode.ToLower()).Name} - {folderName}\{date.Year}\Semana {weekNumber} {week}\{salesAgentName}";

                response = CreateDirectoryIfNotExists(folderPath, 2).Result;
                if (!response.Ok)
                {
                    return EntityResponse.CreateError($"{response.Mensaje}");
                }
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError($"Error en metodo GetReportsFolderPath: {ex.Message}.");
            }

            return EntityResponse.CreateOk(folderPath);
        }

        public async Task<EntityResponse> CreateDirectoryIfNotExists(string path, int lastPositionOfPath)
        {
            try
            {
                string[] folders = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

                string currentPath = @"\\";

                for (int i = 0; i < lastPositionOfPath; i++)
                {
                    currentPath += folders[i] + @"\\";
                }

                for (int i = lastPositionOfPath; i < folders.Length; i++)
                {
                    currentPath = Path.Combine(currentPath, folders[i]);

                    // Check if the current part of the path exists
                    if (!Directory.Exists(currentPath))
                    {
                        // Create the directory if it doesn't exist
                        Directory.CreateDirectory(currentPath);
                    }
                }

                return EntityResponse.CreateOk(path);

            }
            catch(Exception ex)
            {
                return EntityResponse.CreateError($"Error en metodo CreateDirectoryIfNotExists: {ex.Message}.");
            }
        }

        public async Task<EntityResponse> CopyExcelBook(string sourcePath, string destinationPath, string fileName, string sheetName)
        {
            try
            {
                string extension = Path.GetExtension(sourcePath);
                destinationPath += $@"\{fileName}{extension}";

                System.IO.File.Copy(sourcePath, destinationPath, true);

                using (ExcelPackage package = new (destinationPath))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    worksheet.Name = sheetName;

                    FileInfo destinationFile = new (destinationPath);
                    package.SaveAs(destinationFile);
                }

                return EntityResponse.CreateOk(destinationPath);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError($"Error en metodo CopyExcelBook: {ex.Message}.");
            }
        }
    }
}