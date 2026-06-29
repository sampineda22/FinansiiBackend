using Aspose.Cells;
using ClosedXML.Excel;
using CRM.Features.Accounting.AccountingConfiguration;
using CRM.Features.Admin.Users;
using Microsoft.Data.SqlClient;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CRM.Features.Accounting.VendPaymentReport
{
    public class VendPaymentReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AccountingConfigurationService _accountingConfigurationService;

        public VendPaymentReportService(IUnitOfWork unitOfWork, AccountingConfigurationService accountingConfigurationService)
        {
            _unitOfWork = unitOfWork;
            _accountingConfigurationService = accountingConfigurationService;
        }

        public EntityResponse GetJournalsUnposted(string companyCode)
        {
            EntityResponse response = new();

            try
            {
                SqlParameter[] parameters =
                {
                  new SqlParameter("@CompanyCode",companyCode)
                };

                List<Journal> journals = _unitOfWork.Repository<Journal>().GetSP<Journal>("[Finansii].[GetUnpostedJournal]", parameters).ToList();

                if(journals == null)
                {
                    return EntityResponse.CreateError("No se pudieron obtener los diarios abiertos.");
                }

                return EntityResponse.CreateOk(journals);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en metodo GetJournalsUnposted: " + ex.ToString());
            }
        }

        public EntityResponse GetVendPaymentLines(string journalNum, string companyCode)
        {
            try
            {
                SqlParameter[] parameters =
                {
                  new SqlParameter("@JournalNum",journalNum),
                  new SqlParameter("@CompanyCode",companyCode)
                };

                List<JournalLine> lines = _unitOfWork.Repository<JournalLine>().GetSP<JournalLine>("[Finansii].[GetVendJournalLines]", parameters).ToList();

                if (lines == null)
                {
                    return EntityResponse.CreateError("No se pudieron obtener las lineas del diario");
                }
                return EntityResponse.CreateOk(lines);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en metodo GetVendPaymentLines: " + ex.ToString());
            }
        }

        public EntityResponse GetHeader(string journalNum, string companyCode, string offSetLedgerDimension)
        {
            try
            {
                SqlParameter[] parameters =
                {
                  new SqlParameter("@JournalNum",journalNum),
                  new SqlParameter("@CompanyCode",companyCode),
                  new SqlParameter("@OffSetLedgerDimension", offSetLedgerDimension)
                };

                Header header = _unitOfWork.Repository<Header>().GetSP<Header>("[Finansii].[GetHeaderOfVendReport]", parameters).FirstOrDefault();

                if (header == null)
                {
                    return EntityResponse.CreateError("No se pudo obtener el encabezado del reporte");
                }

                return EntityResponse.CreateOk(header);

            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en metodo GetHeader: " + ex.ToString());
            }
        }

        public EntityResponse CreateReport(string journalNum, string companyCode, string user)
        {
            try
            {
                EntityResponse response = new();
                UserDto userDto = new();
                List<JournalLine> journalLines = new();
                List<PaymentDate> paymentDates = new();
                Header header = new();
                using var workbook = new XLWorkbook();
                using var memoryStream = new MemoryStream();
                List<string> warnings = new();
                string[] headers = { "Fecha de Pago:", "Banco/Cuenta:", "Proveedores a Pagar", "Beneficiarios:", "Monto a Pagar:" };
                string[] headerValues = new string[headers.Length];
                string[] columns= { "Linea", "Asiento", "Nombre de la Cuenta", "Descripción", "Débito", "Estado de Pago" };
                string logoPath = @$"//GIM-SER-FINANZAS/Logos/", headerWithoutValue = "Proveedores a Pagar", totalAmountHeader = "Monto a Pagar:", warningsJoin = "";
                int initialRow = 5, row = 0, lineCounter = 1, headerRow = 0, debitColumnNum = 5, headerSecondColumnRow = 0, lastColumnNumber = columns.Length, startDate = 0, endDate = 0;
                byte[] pdfBytes = null;

                response = GetVendPaymentLines(journalNum, companyCode);
                if (response is EntityResponse<List<JournalLine>> genericResponse)
                {
                    journalLines = genericResponse.Data;

                }
                else if (!response.Ok)
                {
                    return EntityResponse.CreateError(response.Mensaje);
                }

                var currencies = journalLines.Select(x => x.CurrencyCode).GroupBy(x => x).ToList();
                if (currencies.Count() > 1)
                {
                    string currencyCodes = string.Join(", ", journalLines
                                                .Select(x => x.CurrencyCode)
                                                .GroupBy(x => x)
                                                .Select(g => g.Key));

                    warnings.Add($"Se encontraron {currencyCodes.Count()} tipos de moneda en el diario. Las monedas encontradas son: " + currencyCodes);
                }

                var cuentaContrapartida = journalLines.Select(x => x.OffSetLedgerDimension).GroupBy(x => x).ToList();
                if (cuentaContrapartida.Count() > 1)
                {
                    string cuentas = string.Join(", ", journalLines
                                                .Select(x => x.OffSetLedgerDimension)
                                                .GroupBy(x => x)
                                                .Select(g => g.Key));

                    warnings.Add($"Se encontraron {cuentaContrapartida.Count()} cuentas de contrapartida diferentes. Favor de validar el diario");
                }

                response = GetHeader(journalNum, companyCode, journalLines.Select(x => x.OffSetLedgerDimension).First());
                if (response is EntityResponse<Header> genericResponse2)
                {
                    header = genericResponse2.Data;
                }
                else if (!response.Ok)
                {
                    return EntityResponse.CreateError(response.Mensaje);
                }

                response = _accountingConfigurationService.GetPaymentDates(companyCode);
                if (response is EntityResponse<List<PaymentDate>> genericResponse3)
                {
                    paymentDates = genericResponse3.Data;
                    paymentDates = paymentDates.Where(x => x.Year == DateTime.Now.Year && x.Month == (DateTime.Now.Month -1)).ToList();

                    if (paymentDates.Count <= 0)
                    {
                        warnings.Add("No se encontro una fecha de pago configurada para el presente mes.");
                    }
                    else
                    {
                        startDate = paymentDates.Select(x => x.StartDate).First();
                        endDate = paymentDates.Select(x => x.EndDate).First();
                    }
                }
                else if (!response.Ok)
                {
                    warnings.Add(response.Mensaje);
                }

                response = GetUserInfo(companyCode, user);
                if (response is EntityResponse<UserDto> genericResponse4)
                {
                    userDto = genericResponse4.Data;
                }
                else if (!response.Ok)
                {
                    warnings.Add(response.Mensaje);
                }

                var worksheet = workbook.Worksheets.Add("Sheet1");

                row = initialRow;
                worksheet.Range(1, 1, 3, lastColumnNumber).Merge();
                worksheet.Cell("A1").Value = "Pago a Proveedores";
                worksheet.Cell("A1").Style.Font.Bold = true;
                worksheet.Cell("A1").Style.Font.FontSize = 18;
                worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                for (int x = 0; x < headers.Length; x++)
                {
                    if (headers[x].Contains(headerWithoutValue))
                    {
                        row++;
                        worksheet.Cell($"A{row}").Style.Font.Bold = true;
                    }

                    if(headers.Length - x == 2)
                    {
                        headerSecondColumnRow = row;
                    }
                    worksheet.Cell($"A{row}").Value = headers[x];
                    worksheet.Cell($"A{row}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    row++;
                }

                headerValues[0] = $"Del {startDate} al {endDate} de {DateTime.Now.ToString("MMMM", new System.Globalization.CultureInfo("es-ES"))} {DateTime.Now.Year}";
                headerValues[1] = $"{header.Name}/{header.AccountNum}";
                headerValues[2] = "";
                headerValues[3] = $"{journalLines.Select(x => x.Name).GroupBy(x => x).Count()}";
                headerValues[4] = "";

                row = initialRow;
                for (int x = 0; x < headers.Length; x++)
                {
                    if (headers[x].Contains(headerWithoutValue))
                    {
                        row++;
                    }

                    if (headers[x].Contains(totalAmountHeader))
                    {
                        worksheet.Cell($"B{row}").Value = journalLines.Select(x => x.Debit).Sum();
                        worksheet.Cell($"B{row}").Style.NumberFormat.Format = header.CurrencyCode == "HNL" ? "\"L\" #,##0.00" :
                                                                              header.CurrencyCode == "USD" ? "\"$\" #,##0.00" 
                                                                              : "#,##0.00";
                    }
                    else
                    {
                        worksheet.Cell($"B{row}").Value = headerValues[x];
                    }

                    worksheet.Cell($"B{row}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    row++;
                }

                worksheet.Cell(headerSecondColumnRow, lastColumnNumber-1).Value = "Fecha de Elaboración:";
                worksheet.Cell(headerSecondColumnRow, lastColumnNumber - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet.Cell(headerSecondColumnRow+1, lastColumnNumber - 1).Value = "Elaborado por:";
                worksheet.Cell(headerSecondColumnRow+1, lastColumnNumber - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                worksheet.Cell(headerSecondColumnRow, lastColumnNumber).Value = DateTime.Now.ToString("dd-MMM-yyyy", new System.Globalization.CultureInfo("es-ES"));
                worksheet.Cell(headerSecondColumnRow, lastColumnNumber).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(headerSecondColumnRow + 1, lastColumnNumber).Value = userDto != null ? $"{userDto.FirstName} {userDto.FirstLastName}" : user;
                worksheet.Cell(headerSecondColumnRow + 1, lastColumnNumber).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                row++;
                for (int x = 0; x < columns.Length; x++)
                {
                    int columnNumber = x + 1;

                    worksheet.Cell(row, columnNumber).Value = columns[x];
                    worksheet.Cell(row, columnNumber).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                headerRow = row;
                row++;

                foreach (JournalLine line in journalLines)
                {
                    object[] values =
                    {
                        lineCounter,
                        line.Voucher,
                        line.Name,
                        line.Description,
                        line.Debit,
                        line.PaymentStatus,
                    };

                    for (int x = 0; x < values.Length; x++)
                    {
                        int columnNumber = x + 1;
                        var cell = worksheet.Cell(row, columnNumber);

                        switch (values[x])
                        {
                            case int intValue:
                                cell.Value = intValue;
                                break;

                            case decimal decimalValue:
                                cell.Value = decimalValue;
                                break;

                            case string stringValue:
                                cell.Value = stringValue;
                                break;

                            case null:
                                cell.Value = "";
                                break;
                        }
                    }

                    worksheet.Cell(row, debitColumnNum).Style.NumberFormat.Format = line.CurrencyCode == "HNL" ? "\"L\" #,##0.00" :
                                                                                    line.CurrencyCode == "USD" ? "\"$\" #,##0.00"
                                                                                    : "#,##0.00";

                    row++;
                    lineCounter++;
                }

                var tableRange = worksheet.Range(headerRow, 1, (row - 1), lastColumnNumber);
                var table = tableRange.CreateTable();
                table.Theme = XLTableTheme.TableStyleMedium2;
                table.ShowAutoFilter = true;

                response = GetImage(companyCode, logoPath);
                if (!response.Ok)
                {
                    warnings.Add( response.Mensaje);
                }
                else
                {
                    worksheet.AddPicture(response.Mensaje)
                    .MoveTo(worksheet.Cell("A1"))
                    .WithSize(180, 60);
                }

                response = GetImage(header.Name, logoPath);
                if (!response.Ok)
                {
                    warnings.Add(response.Mensaje);
                }
                else
                {
                    var picture = worksheet.AddPicture(response.Mensaje)
                    .MoveTo(worksheet.Cell(1, (lastColumnNumber-1)))
                    .WithSize(180, 60);
                }

                worksheet.Columns().AdjustToContents();
                worksheet.Rows().AdjustToContents();

                workbook.SaveAs(memoryStream);
                memoryStream.Position = 0;

                // Save memory file to Desktop                
               /* string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(desktopPath, "Pago a Proveedores.xlsx");

                File.WriteAllBytes(filePath, memoryStream.ToArray());*/

                if (warnings.Count > 0) {
                    warningsJoin = "Reporte guardado. Durante el reporte, se encontraron las siguientes advertencias: " + string.Join(", ", warnings);
                }

                response = SaveExcelAsPDF(workbook, "ReportePago");
                if (response is EntityResponse<byte[]> genericResponse5)
                {
                    pdfBytes = genericResponse5.Data;
                }
                else if (!response.Ok)
                {
                    return EntityResponse.CreateError(response.Mensaje);
                }

                var result = new ReportResult
                {
                    PdfBytes = pdfBytes,
                    Warnings = warningsJoin
                };

                return EntityResponse.CreateOk(result);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en método CreateReport: " + ex.ToString());
            }
        }

        public string GetFileName(string journalNum, string companyCode, string offSetLedgerDimension)
        {
            Header header = new();
            EntityResponse response = GetHeader(journalNum, companyCode, offSetLedgerDimension);

            if (response is EntityResponse<Header> genericResponse)
            {
                header = genericResponse.Data;
            }
            else if (!response.Ok)
            {
                return $"Pago a Proveedores {DateTime.Now.ToString("MMMM", new System.Globalization.CultureInfo("es-ES"))}";
            }

            return $"Pago a Proveedores {DateTime.Now.ToString("MMMM", new System.Globalization.CultureInfo("es-ES"))} - {header.Name}";
        }

        public EntityResponse GetUserInfo(string companyCode, string userId)
        {
            try
            {
                SqlParameter[] parameters =
                {
                  new SqlParameter("@companyCode",companyCode)
                };

                List<UserDto> users = _unitOfWork.Repository<UserDto>().GetSP<UserDto>("[Finansii].[GetUsersInfo]", parameters).ToList();
                UserDto user = users.Find(x => x.UserId == userId);

                if (user == null) {
                    return EntityResponse.CreateError("No se pudo obtener la información del usuario");
                }

                return EntityResponse.CreateOk(user);

            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en metodo GetUserInfo: " + ex.ToString());
            }
        }

        public EntityResponse SaveExcelAsPDF(XLWorkbook workbook, string fileName)
        {
            string tempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                Directory.CreateDirectory(tempFolder);

                string excelPath = System.IO.Path.Combine(tempFolder, $"{fileName}.xlsx");
                string pdfPath = System.IO.Path.Combine(tempFolder, $"{fileName}.pdf");

                workbook.SaveAs(excelPath);

                Aspose.Cells.Workbook aposeWorkbook = new (excelPath);

                foreach (Aspose.Cells.Worksheet sheet in aposeWorkbook.Worksheets)
                {
                    sheet.PageSetup.Orientation = PageOrientationType.Landscape;
                    sheet.PageSetup.PaperSize = PaperSizeType.PaperLetter;
                }

                PdfSaveOptions options = new PdfSaveOptions();
                options.AllColumnsInOnePagePerSheet = true;
                options.CalculateFormula = true;

                aposeWorkbook.Save(pdfPath, options);
                byte[] pdfBytes = File.ReadAllBytes(pdfPath);

                return EntityResponse.CreateOk(pdfBytes);
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                    Directory.Delete(tempFolder, true);
            }
        }

        public EntityResponse GetImage(string imageName, string path)
        {
            try
            {
                EntityResponse response = new();
                string[] extensions = { ".png", ".jpg", ".jpeg" };

                string imagePath = extensions
                .Select(ext => System.IO.Path.Combine(path, imageName + ext))
                .FirstOrDefault(File.Exists);

                if (imagePath != null)
                {
                    response.Mensaje = imagePath;
                    response.Ok = true;
                    return response;
                }
                else
                {
                    response.Mensaje = "La imagen no fue encontrada";
                    response.Ok = false;
                    return response;
                }
            }
            catch (Exception ex) {
                return EntityResponse.CreateError("Error en metodo GetImage: " + ex.ToString());
            }
        }
    }
}
