using CRM.Features.Credits.ReceiptBreakdownReport;
using CRM.Features.Credits.ReceiptDetailBreakdownReport;
using CRM.Infrastructure.Core;
using CRM.Models.General;
using Finansii.Reports.PdfImport;
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

        public async Task<EntityResponse> CreateReceiptDetailBreakdownReports(string start, string end, string weekNumber, string companyCode, string salesAgentSelected)
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
                List<SalesAgent> allSalesAgents = _unitOfWork.Repository<SalesAgent>().GetSP<SalesAgent>("[Finansii].[GetSalesAgents]", parameters, 400).ToList();

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

                foreach (SalesAgent agent in salesAgents)
                {
                    int advanceTableRow = 0;
                    int tableRow = 10;

                    parameters = new SqlParameter[]
                    {
                            new SqlParameter("@StartDate", startDate),
                            new SqlParameter("@EndDate", endDate),
                            new SqlParameter("@PersonalCode", agent.PersonalCode),
                            new SqlParameter("@DataAreaId", companyCode),
                            new SqlParameter("@DataAreaOfAgent", agent.AgentCompanyCode)
                    };
                    List<ReceiptDetailBreakdown> receiptDetailBreakdown = _unitOfWork.Repository<ReceiptDetailBreakdown>().GetSP<ReceiptDetailBreakdown>("[Finansii].[ReceiptDetailBreakdown]", parameters, 600).ToList();

                    if(receiptDetailBreakdown.Count > 0)
                    {
                        response = _workpaperReportService.GetReportsFolderPath(companyCode
                                                                                /*Commented on 2026-ene.-06 by spineda - Begin*/
                                                                                , endDate
                                                                                /*Commented on 2025-ene.-06 by spineda - End*/
                                                                                , serverPath, "Cédulas de Asesores de Venta", agent.Name, week, weekNumber).Result;
                        if (response is EntityResponse<string> genericResponse2)
                        {
                            folderPath = genericResponse2.Data;
                        }
                        else
                        {
                            return EntityResponse.CreateError($"{response.Mensaje}");
                        }

                        string pdfFilePath = $@"{folderPath}\{"Reporte detalle de recibos -" + agent.PersonalCode}.pdf";

                        response = _workpaperReportService.CopyExcelBook(templatePath, folderPath, "Desglose de Recibos -" + agent.PersonalCode, "Reporte").Result;
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
                            string receipt = "";

                            targetWorksheet.Cells[$"C2"].Value = companies.Find(x => x.CompanyCode.ToLower() == companyCode.ToLower()).Name;
                            targetWorksheet.Cells[$"C4"].Value = startDate.Year.ToString(); //Año
                            targetWorksheet.Cells[$"F4"].Value = agent.Name; //Asesor
                            targetWorksheet.Cells[$"C5"].Value = weekNumber; //Semana
                            targetWorksheet.Cells[$"C6"].Value = week; //Fecha

                            table = targetWorksheet.Tables["TablaDesglose"];

                            foreach (ReceiptDetailBreakdown detail in receiptDetailBreakdown)
                            {
                                targetWorksheet.Cells[$"B{tableRow}"].Value = receipt == detail.ReceiptNumber ? "" : detail.ReceiptNumber;
                                targetWorksheet.Cells[$"C{tableRow}"].Value = detail.DocumentNumber;
                                targetWorksheet.Cells[$"D{tableRow}"].Value = detail.FELDocument;
                                targetWorksheet.Cells[$"E{tableRow}"].Value = detail.ProductType;
                                targetWorksheet.Cells[$"F{tableRow}"].Value = detail.Date.ToString("dd-MMM-yyyy", new System.Globalization.CultureInfo("es-ES"));
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

                                targetWorksheet = _workpaperReportService.ApplyBorderToReport(tableRow, targetWorksheet, 'P', receipt != detail.ReceiptNumber);
                                receipt = detail.ReceiptNumber;

                                tableRow++;
                                targetWorksheet.InsertRow(tableRow, 1);
                            }
                            targetWorksheet.DeleteRow(tableRow, 1);

                            var newAddress = new ExcelAddressBase(table.Address.Start.Row, table.Address.Start.Column, tableRow, table.Address.End.Column);
                            typeof(ExcelTable).GetProperty("Address").SetValue(table, newAddress);

                            advanceTableRow = tableRow + spaceBetweenTables;

                            /*parameters = new SqlParameter[]
                            {
                            new SqlParameter("@SalesAgent", agent.PersonalCode),
                            new SqlParameter("@StartDate", startDate),
                            new SqlParameter("@EndDate", endDate),
                            new SqlParameter("@DataAreaId", companyCode)
                            };
                            List<AppliedAdvance> appliedAdvances = _unitOfWork.Repository<AppliedAdvance>().GetSP<AppliedAdvance>("[Finansii].[GetAdvancesWithInvoices]", parameters).ToList();

                            foreach (AppliedAdvance appliedAdvance in appliedAdvances)
                            {//Esta comentado hasta que se resuelva lo de los anticipos
                                targetWorksheet.Cells[$"B{advanceTableRow}"].Value = appliedAdvance.AdvanceReceipt;
                                targetWorksheet.Cells[$"C{advanceTableRow}"].Value = appliedAdvance.AppliedAdvanceAmount;
                                targetWorksheet.Cells[$"C{advanceTableRow}"].Style.Numberformat.Format = "#,##0.00";
                                targetWorksheet.Cells[$"C{advanceTableRow}"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                                targetWorksheet.Cells[$"D{advanceTableRow}"].Value = appliedAdvance.Invoice;*/

                                advanceTableRow++;
                                targetWorksheet.InsertRow(advanceTableRow, 1);
                            /*}*/ // Esta comentado hasta que se resuelva lo de los anticipos

                            targetWorksheet.DeleteRow(advanceTableRow, 1);

                            table = targetWorksheet.Tables["TablaAnticipos"];
                            newAddress = new ExcelAddressBase(table.Address.Start.Row, table.Address.Start.Column, advanceTableRow, table.Address.End.Column);
                            typeof(ExcelTable).GetProperty("Address").SetValue(table, newAddress);

                            var receipts = receiptDetailBreakdown.Select(e => e.ReceiptNumber.Replace(" ", "")).Distinct();
                            string documentsNum = string.Join(",", receipts);

                            parameters = new SqlParameter[]
                            {
                            new SqlParameter("@DocumentsNum", documentsNum),
                            new SqlParameter("@DataAreaId", companyCode)
                            };
                            List<JournalLine> journalLines = _unitOfWork.Repository<JournalLine>().GetSP<JournalLine>("[Finansii].[GetJournalsByDocumentNum]", parameters).ToList();

                            targetWorksheet.Cells[$"C{advanceTableRow + spaceBetweenSignature}"].Value = journalLines.Count <= 0 ? "" : string.Join(", ", journalLines.Select(x => x.ModifiedBy).Distinct());

                            /*Commented on 2026-jun.-23 by spineda - Begin*/
                            targetWorksheet.Column(2).AutoFit();
                            var col = targetWorksheet.Column(12);
                            col.AutoFit();
                            col.Width += 2;
                            col = targetWorksheet.Column(13);
                            col.AutoFit();
                            col.Width += 2;
                            /*Commented on 2026-jun.-23 by spineda - End*/

                            package.Save();
                        }
                        
                        _workpaperReportService.ConvertExcelToPdf(excelPath, pdfFilePath);
                        File.Delete(excelPath);
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
                     new SqlParameter("@EmpresaId", companyCode),
                };
                List<SalesAgent> salesAgents = _unitOfWork.Repository<SalesAgent>().GetSP<SalesAgent>("[Finansii].[GetSalesAgents]", parameters, 250).ToList();

                parameters = new SqlParameter[]
                {
                   new SqlParameter("@StartDate", startDate),
                   new SqlParameter("@EndDate", endDate),
                   new SqlParameter("@PersonalCode", salesmanCode),
                   new SqlParameter("@DataAreaId", companyCode),
                   new SqlParameter("@DataAreaOfAgent", salesAgents.Find(x => x.PersonalCode == salesmanCode).AgentCompanyCode),
                };
                receiptDetailBreakdown = _unitOfWork.Repository<ReceiptDetailBreakdown>().GetSP<ReceiptDetailBreakdown>("[Finansii].[ReceiptDetailBreakdown]", parameters, commandTimeout: 300).ToList();

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

        private const int ReceiptTableFirstRow = 10;
        private const int ReceiptSpaceBetweenTables = 5;
        private const int ReceiptSpaceBetweenSignature = 2;

        #region opcion 1: un Excel por cada PDF

        /// <summary>
        /// Genera el Excel de "Desglose Detalle de Recibos" leyendo los datos de los PDF
        /// ya existentes, en lugar del stored procedure. El .xlsx se guarda junto al PDF.
        /// </summary>
        /// <param name="rootPath">
        /// Carpeta raiz, p. ej.
        /// \\10.100.0.41\boveda de documentos\Facturacion\INTERMODA SA DE CV - Cedulas de Asesores de Venta\2026
        /// </param>
        /// <param name="subfolders">
        /// Subcarpetas a recorrer dentro de rootPath, p. ej.
        /// { "Semana 2 05-01-2026 al 11-01-2026", "Semana 3 12-01-2026 al 18-01-2026" }.
        /// Se busca de forma recursiva dentro de cada una (las carpetas de cada asesor).
        /// Si viene vacia o null, se recorre rootPath completo.
        /// </param>
        /// <param name="fileNamePrefix">Solo se convierten los .pdf cuyo nombre empieza con este texto.</param>
        /// <param name="overwriteExisting">Si false, salta los PDF que ya tienen su .xlsx generado.</param>
        public async Task<EntityResponse> CreateReceiptDetailBreakdownExcelFromPdf(
            List<string> subfolders,
            string fileNamePrefix = "Reporte detalle de recibos -",
            bool overwriteExisting = true)
        {
            try
            {
                string rootPath = @"\\10.100.0.41\boveda de documentos\Facturacion\INTERMODA SA DE CV - Cédulas de Asesores de Venta\2026";
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                EntityResponse response = new();

                int generated = 0, skipped = 0;
                List<string> problems = new();

                EntityResponse validation = ValidateReceiptPdfInputs(rootPath, fileNamePrefix, out string templatePath);
                if (validation != null) return validation;

                List<string> pdfFiles = FindReceiptPdfFiles(rootPath, subfolders, fileNamePrefix, problems, out string discoveryError);
                if (discoveryError != null) return EntityResponse.CreateError(discoveryError);

                foreach (string pdfPath in pdfFiles)
                {
                    try
                    {
                        // el .xlsx se guarda en la misma carpeta del PDF
                        string folderPath = Path.GetDirectoryName(pdfPath);
                        string bookName = Path.GetFileNameWithoutExtension(pdfPath);
                        string expectedExcelPath = Path.Combine(folderPath, $"{bookName}.xlsx");

                        if (File.Exists(expectedExcelPath) && !overwriteExisting)
                        {
                            skipped++;
                            continue;
                        }

                        ReceiptPdfReport report = ReceiptPdfParser.Parse(pdfPath);

                        if (report.Details.Count == 0)
                        {
                            // se reporta la ruta completa para poder ubicar el PDF
                            problems.Add($"{pdfPath}: el PDF no trae filas de detalle.");
                            skipped++;
                            continue;
                        }

                        // report.Notes son informativas (ajuste de columnas) y no se reportan
                        foreach (string warning in report.Warnings) problems.Add($"{pdfPath}: {warning}");

                        if (File.Exists(expectedExcelPath)) File.Delete(expectedExcelPath);

                        string excelPath;
                        response = await _workpaperReportService.CopyExcelBook(templatePath, folderPath, bookName, "Reporte");

                        if (response is EntityResponse<string> copyResponse)
                        {
                            excelPath = copyResponse.Data;
                        }
                        else
                        {
                            problems.Add($"{pdfPath}: {response.Mensaje}");
                            skipped++;
                            continue;
                        }

                        FileInfo sourceFile = new(excelPath);

                        using (ExcelPackage package = new(sourceFile))
                        {
                            FillReceiptWorksheet(package.Workbook.Worksheets[0], report);
                            package.Save();
                        }

                        generated++;
                    }
                    catch (Exception ex)
                    {
                        problems.Add($"{pdfPath}: {ex.Message}");
                    }
                }

                string message = $"Se generaron {generated} de {pdfFiles.Count} archivos Excel.";
                if (skipped > 0) message += $" Se omitieron {skipped}.";
                message += BuildProblemsSummary(problems);

                return generated == 0 ? EntityResponse.CreateError(message) : EntityResponse.CreateOk(message);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError($"Error en metodo CreateReceiptDetailBreakdownExcelFromPdf: {ex.Message}.");
            }
        }

        #endregion

        #region opcion 2: un solo Excel con una hoja por reporte

        /// <summary>
        /// Igual que CreateReceiptDetailBreakdownExcelFromPdf pero deja todos los
        /// reportes en un solo libro, una hoja por PDF. La hoja se nombra con el asesor
        /// y la semana que trae el propio PDF, p. ej. "Oficina honduras S27".
        /// </summary>
        /// <param name="rootPath">Carpeta raiz donde estan las subcarpetas por semana.</param>
        /// <param name="subfolders">Subcarpetas a recorrer. Si viene null o vacia se recorre rootPath completo.</param>
        /// <param name="outputFilePath">
        /// Ruta del .xlsx a generar. Si se indica una carpeta, se crea dentro con el
        /// nombre "Desglose Detalle de Recibos.xlsx".
        /// </param>
        /// <param name="fileNamePrefix">Solo se leen los .pdf cuyo nombre empieza con este texto.</param>
        /// <param name="overwriteExisting">Si false y el archivo de salida ya existe, no hace nada.</param>
        public async Task<EntityResponse> CreateReceiptDetailBreakdownWorkbookFromPdf(
            List<string> subfolders,
            string outputFilePath,
            string fileNamePrefix = "Reporte detalle de recibos -",
            bool overwriteExisting = true)
        {
            try
            {
                string rootPath = @"\\10.100.0.41\boveda de documentos\Facturacion\INTERMODA SA DE CV - Cédulas de Asesores de Venta\2026";
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                int added = 0, skipped = 0;
                List<string> problems = new();

                EntityResponse validation = ValidateReceiptPdfInputs(rootPath, fileNamePrefix, out string templatePath);
                if (validation != null) return validation;

                if (string.IsNullOrWhiteSpace(outputFilePath))
                    return EntityResponse.CreateError("Debe indicar la ruta del archivo Excel de salida (outputFilePath).");

                // si dieron una carpeta, se arma el nombre del archivo
                if (Directory.Exists(outputFilePath) ||
                    string.IsNullOrEmpty(Path.GetExtension(outputFilePath)))
                {
                    outputFilePath = Path.Combine(outputFilePath, "Desglose Detalle de Recibos.xlsx");
                }

                string outputFolder = Path.GetDirectoryName(outputFilePath);
                if (!string.IsNullOrEmpty(outputFolder)) Directory.CreateDirectory(outputFolder);

                if (File.Exists(outputFilePath) && !overwriteExisting)
                    return EntityResponse.CreateError($"El archivo ya existe y no se pidio reemplazarlo: {outputFilePath}");

                List<string> pdfFiles = FindReceiptPdfFiles(rootPath, subfolders, fileNamePrefix, problems, out string discoveryError);
                if (discoveryError != null) return EntityResponse.CreateError(discoveryError);

                // se lee todo primero: asi no se deja un libro a medias si algun PDF falla
                List<ReceiptPdfReport> reports = new();

                foreach (string pdfPath in pdfFiles)
                {
                    try
                    {
                        ReceiptPdfReport report = ReceiptPdfParser.Parse(pdfPath);

                        if (report.Details.Count == 0)
                        {
                            // se reporta la ruta completa para poder ubicar el PDF
                            problems.Add($"{pdfPath}: el PDF no trae filas de detalle.");
                            skipped++;
                            continue;
                        }

                        foreach (string warning in report.Warnings) problems.Add($"{pdfPath}: {warning}");
                        reports.Add(report);
                    }
                    catch (Exception ex)
                    {
                        problems.Add($"{pdfPath}: {ex.Message}");
                        skipped++;
                    }
                }

                if (reports.Count == 0)
                    return EntityResponse.CreateError($"No se pudo leer ningun PDF.{BuildProblemsSummary(problems)}");

                // Las hojas quedan ordenadas por semana de menor a mayor. Se ordena por
                // numero y no por texto: si no, "Semana 10" iria antes de "Semana 2".
                reports = reports
                    .OrderBy(r => ParseNumberOrMax(r.Year))
                    .ThenBy(r => ParseNumberOrMax(r.WeekNumber))
                    .ThenBy(r => r.AgentName ?? "", StringComparer.CurrentCultureIgnoreCase)
                    .ToList();

                if (File.Exists(outputFilePath)) File.Delete(outputFilePath);
                File.Copy(templatePath, outputFilePath);

                using (ExcelPackage package = new(new FileInfo(outputFilePath)))
                {
                    ExcelWorksheet templateSheet = package.Workbook.Worksheets[0];
                    HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);
                    int index = 0;

                    foreach (ReceiptPdfReport report in reports)
                    {
                        string sheetName = BuildReceiptSheetName(report, usedNames);

                        try
                        {
                            // se copia la hoja del template para conservar formato y tablas
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName, templateSheet);
                            index++;

                            // al copiar la hoja, EPPlus le pone nombres nuevos a las tablas
                            // ("Table1", "Table2"), asi que aqui se les vuelve a dar un
                            // nombre unico y estable. Nunca se buscan por nombre.
                            RenameReceiptTables(worksheet, index);

                            FillReceiptWorksheet(worksheet, report);
                            added++;
                        }
                        catch (Exception ex)
                        {
                            problems.Add($"{report.FilePath}: no se pudo agregar la hoja '{sheetName}': {ex.Message}");
                            skipped++;
                        }
                    }

                    if (added > 0)
                    {
                        // la hoja original del template ya no se necesita
                        package.Workbook.Worksheets.Delete(templateSheet);
                        package.Workbook.Worksheets[0].Select();
                        package.Save();
                    }
                }

                if (added == 0)
                {
                    // el archivo se borra ya cerrado el paquete, si no lanza IOException
                    if (File.Exists(outputFilePath)) File.Delete(outputFilePath);
                    return EntityResponse.CreateError($"No se agrego ninguna hoja.{BuildProblemsSummary(problems)}");
                }

                string message = $"Se genero el archivo con {added} hojas de {pdfFiles.Count} PDF encontrados: {outputFilePath}";
                if (skipped > 0) message += $" Se omitieron {skipped}.";
                message += BuildProblemsSummary(problems);

                return EntityResponse.CreateOk(message);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError($"Error en metodo CreateReceiptDetailBreakdownWorkbookFromPdf: {ex.Message}.");
            }
        }

        #endregion

        #region llenado compartido

        /// <summary>
        /// Escribe un reporte en la hoja indicada, con la misma estructura del template:
        /// encabezado, tabla de desglose, tabla de anticipos y "Registrado por".
        /// </summary>
        private void FillReceiptWorksheet(ExcelWorksheet targetWorksheet, ReceiptPdfReport report)
        {
            string receipt = "";
            int tableRow = ReceiptTableFirstRow;
            int advanceTableRow;

            targetWorksheet.Cells["C2"].Value = report.CompanyName;   // Empresa
            targetWorksheet.Cells["C4"].Value = report.Year;          // Anio
            targetWorksheet.Cells["F4"].Value = report.AgentName;     // Asesor
            targetWorksheet.Cells["C5"].Value = report.WeekNumber;    // Semana
            targetWorksheet.Cells["C6"].Value = report.DateRange;     // Fecha

            // Las tablas se localizan por su fila de inicio y no por nombre: en el libro
            // consolidado las copias de la hoja pierden los nombres originales.
            GetReceiptTables(targetWorksheet, out ExcelTable breakdownTable, out ExcelTable advancesTable);

            foreach (ReceiptPdfRow detail in report.Details)
            {
                targetWorksheet.Cells[$"B{tableRow}"].Value = receipt == detail.ReceiptNumber ? "" : detail.ReceiptNumber;
                targetWorksheet.Cells[$"C{tableRow}"].Value = detail.DocumentNumber;
                targetWorksheet.Cells[$"D{tableRow}"].Value = detail.FELDocument;
                targetWorksheet.Cells[$"E{tableRow}"].Value = detail.ProductType;
                // se escribe el texto tal cual viene del PDF: el metodo original tambien
                // guardaba la fecha ya formateada como texto
                targetWorksheet.Cells[$"F{tableRow}"].Value = detail.DateText;
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

                targetWorksheet = _workpaperReportService.ApplyBorderToReport(tableRow, targetWorksheet, 'P', receipt != detail.ReceiptNumber);
                receipt = detail.ReceiptNumber;

                tableRow++;
                targetWorksheet.InsertRow(tableRow, 1);
            }
            targetWorksheet.DeleteRow(tableRow, 1);

            var newAddress = new ExcelAddressBase(breakdownTable.Address.Start.Row, breakdownTable.Address.Start.Column,
                                                  tableRow, breakdownTable.Address.End.Column);
            typeof(ExcelTable).GetProperty("Address").SetValue(breakdownTable, newAddress);

            // ------------------------------------------------- Anticipos Asignados
            advanceTableRow = tableRow + ReceiptSpaceBetweenTables;

            foreach (ReceiptPdfAdvanceRow advance in report.Advances)
            {
                targetWorksheet.Cells[$"B{advanceTableRow}"].Value = advance.AdvanceReceipt;
                targetWorksheet.Cells[$"C{advanceTableRow}"].Value = advance.AppliedAdvanceAmount;
                targetWorksheet.Cells[$"C{advanceTableRow}"].Style.Numberformat.Format = "#,##0.00";
                targetWorksheet.Cells[$"C{advanceTableRow}"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                targetWorksheet.Cells[$"D{advanceTableRow}"].Value = advance.Invoice;

                advanceTableRow++;
                targetWorksheet.InsertRow(advanceTableRow, 1);
            }

            if (report.Advances.Count > 0)
            {
                targetWorksheet.DeleteRow(advanceTableRow, 1);
            }
            else
            {
                // sin anticipos se mantiene el comportamiento del metodo original:
                // la tabla queda con una sola fila vacia
                advanceTableRow++;
                targetWorksheet.InsertRow(advanceTableRow, 1);
                targetWorksheet.DeleteRow(advanceTableRow, 1);
            }

            newAddress = new ExcelAddressBase(advancesTable.Address.Start.Row, advancesTable.Address.Start.Column,
                                              advanceTableRow, advancesTable.Address.End.Column);
            typeof(ExcelTable).GetProperty("Address").SetValue(advancesTable, newAddress);

            // --------------------------------------------------- Registrado por
            targetWorksheet.Cells[$"C{advanceTableRow + ReceiptSpaceBetweenSignature}"].Value = report.RegisteredBy;

            targetWorksheet.Column(2).AutoFit();
            var col = targetWorksheet.Column(12);
            col.AutoFit();
            col.Width += 2;
            col = targetWorksheet.Column(13);
            col.AutoFit();
            col.Width += 2;
        }

        /// <summary>
        /// Devuelve las dos tablas del template ordenadas por su fila de inicio:
        /// primero la de desglose y despues la de anticipos.
        /// </summary>
        private static void GetReceiptTables(ExcelWorksheet worksheet, out ExcelTable breakdown, out ExcelTable advances)
        {
            List<ExcelTable> tables = worksheet.Tables.OrderBy(t => t.Address.Start.Row).ToList();

            if (tables.Count < 2)
                throw new InvalidOperationException(
                    $"La hoja '{worksheet.Name}' tiene {tables.Count} tabla(s) y se esperaban 2 " +
                    "(desglose y anticipos). Revise el template.");

            breakdown = tables[0];
            advances = tables[1];
        }

        /// <summary>Da nombres unicos a las tablas de una hoja recien copiada.</summary>
        private static void RenameReceiptTables(ExcelWorksheet worksheet, int index)
        {
            try
            {
                GetReceiptTables(worksheet, out ExcelTable breakdown, out ExcelTable advances);
                breakdown.Name = $"TablaDesglose_{index}";
                advances.Name = $"TablaAnticipos_{index}";
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch
            {
                // si el nombre no se puede cambiar se deja el que puso EPPlus: las tablas
                // se buscan por posicion, el nombre es solo para que el libro sea legible
            }
        }

        #endregion

        #region utilidades

        /// <summary>
        /// Convierte a numero para poder ordenar. Lo que no sea numerico se manda al
        /// final en lugar de romper el ordenamiento.
        /// </summary>
        private static int ParseNumberOrMax(string value)
            => int.TryParse((value ?? "").Trim(), out int n) ? n : int.MaxValue;

        /// <summary>Nombre de hoja "Asesor Snn", recortado a 31 caracteres y sin repetirse.</summary>
        private static string BuildReceiptSheetName(ReceiptPdfReport report, HashSet<string> usedNames)
        {
            string agent = (report.AgentName ?? "").Trim();
            if (agent.Length == 0) agent = Path.GetFileNameWithoutExtension(report.FileName) ?? "Reporte";

            // Excel no admite estos caracteres en el nombre de una hoja
            foreach (char c in new[] { ':', '\\', '/', '?', '*', '[', ']' }) agent = agent.Replace(c, '-');
            agent = agent.Trim();

            string week = (report.WeekNumber ?? "").Trim();
            string suffix = week.Length > 0 ? $" S{week}" : "";

            for (int i = 1; ; i++)
            {
                string tail = i == 1 ? suffix : $"{suffix} ({i})";
                int room = Math.Max(1, 31 - tail.Length);
                string head = agent.Length > room ? agent.Substring(0, room).TrimEnd() : agent;
                string candidate = $"{head}{tail}";

                if (candidate.Length == 0) candidate = $"Reporte {i}";
                if (usedNames.Add(candidate)) return candidate;
            }
        }

        /// <summary>Valida los parametros comunes y obtiene la ruta del template.</summary>
        private EntityResponse ValidateReceiptPdfInputs(string rootPath, string fileNamePrefix, out string templatePath)
        {
            templatePath = null;

            if (string.IsNullOrWhiteSpace(rootPath))
                return EntityResponse.CreateError("Debe indicar la carpeta raiz (rootPath).");

            if (!Directory.Exists(rootPath))
                return EntityResponse.CreateError($"No se encontro la carpeta raiz: {rootPath}");

            if (string.IsNullOrWhiteSpace(fileNamePrefix))
                return EntityResponse.CreateError("Debe indicar el prefijo de los archivos a convertir.");

            templatePath = _unitOfWork.Repository<RoutePath>().Query()
                                      .Where(x => x.Name == "RBTemplate")
                                      .FirstOrDefault()?.URL;

            if (string.IsNullOrWhiteSpace(templatePath))
                return EntityResponse.CreateError("No se encontro la ruta del template 'RBTemplate'.");

            if (!File.Exists(templatePath))
                return EntityResponse.CreateError($"No se encontro el archivo del template: {templatePath}");

            return null;
        }

        /// <summary>
        /// Busca los .pdf que empiezan con el prefijo dentro de las subcarpetas
        /// indicadas, de forma recursiva.
        /// </summary>
        private static List<string> FindReceiptPdfFiles(
            string rootPath,
            List<string> subfolders,
            string fileNamePrefix,
            List<string> problems,
            out string error)
        {
            error = null;
            List<string> targetFolders = new();

            if (subfolders == null || subfolders.Count == 0)
            {
                targetFolders.Add(rootPath);
            }
            else
            {
                foreach (string subfolder in subfolders)
                {
                    if (string.IsNullOrWhiteSpace(subfolder)) continue;

                    string folder = Path.Combine(rootPath, subfolder.Trim());
                    if (Directory.Exists(folder)) targetFolders.Add(folder);
                    else problems.Add($"No existe la subcarpeta '{subfolder}'.");
                }
            }

            if (targetFolders.Count == 0)
            {
                error = $"Ninguna de las subcarpetas indicadas existe. {string.Join(" ", problems)}";
                return new List<string>();
            }

            List<string> pdfFiles = new();

            foreach (string folder in targetFolders)
            {
                pdfFiles.AddRange(Directory
                    .EnumerateFiles(folder, "*.pdf", SearchOption.AllDirectories)
                    .Where(f => Path.GetFileName(f).StartsWith(fileNamePrefix, StringComparison.OrdinalIgnoreCase)));
            }

            pdfFiles = pdfFiles.Distinct(StringComparer.OrdinalIgnoreCase)
                               .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                               .ToList();

            if (pdfFiles.Count == 0)
                error = $"No se encontraron archivos .pdf que comiencen con '{fileNamePrefix}'.";

            return pdfFiles;
        }

        /// <summary>Resume los avisos para el mensaje de la respuesta.</summary>
        private static string BuildProblemsSummary(List<string> problems)
        {
            if (problems == null || problems.Count == 0) return "";

            int show = Math.Min(problems.Count, 20);
            string text = $" Avisos ({problems.Count}): {string.Join(" | ", problems.Take(show))}";
            if (problems.Count > show) text += $" ...y {problems.Count - show} mas.";
            return text;
        }

        #endregion

    }
}