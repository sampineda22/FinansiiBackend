using ClosedXML.Excel;
using CRM.Features.Credits.ReceiptBreakdownReport;
using CRM.Features.Credits.ReceiptDetailBreakdownReport;
using CRM.Features.Gira.ExpensesSettings;
using CRM.Infrastructure.Core;
using CRM.Models.General;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace CRM.Features.Gira.Historical
{
    public class HistoricalService
    {
        private readonly IUnitOfWorkGira _unitOfWorkGira;
        private readonly IUnitOfWork _unitOfWork;
        public HistoricalService(IUnitOfWorkGira unitOfWorkGira, IUnitOfWork unitOfWork)
        {
            _unitOfWorkGira = unitOfWorkGira;
            _unitOfWork = unitOfWork;
        }

        public async Task<EntityResponse> GetHistoricalDetails(string companyCode, string? personalCode, int expenseType, DateTime startDate, DateTime endDate/*, bool filterByAdmin*/)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@companyCode", companyCode),
                    new SqlParameter("@startDate", startDate),
                    new SqlParameter("@endDate", endDate),
                    new SqlParameter("@personalCode", (object?)personalCode ?? DBNull.Value),
                    new SqlParameter("@idExpenseType", expenseType)/*,
                    new SqlParameter("@filterByAdmin", filterByAdmin)*/
                };

                List<ExpenseDetailDto> details = _unitOfWork.Repository<ExpenseDetailDto>().GetSP<ExpenseDetailDto>("[Gira].[GetExpensesDetailsByFilters]", parameters).ToList();

                return EntityResponse.CreateOk(details);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en GetHistoricalDetails: " + ex.Message);
            }
        }

        public EntityResponse DownloadExcel(string companyCode, string salesAgent, int expenseType, DateTime startDate, DateTime endDate)
        {
            try
            {
                List<ExpenseDetail> expenseDetails = new();
                using var workbook = new XLWorkbook();
                SqlParameter[] parameters = { };
                IXLRanges dataRanges;
                IXLRange dataRange;
                int column = 2, row = 0, firstRow = 11, finalRow = 0;
                string[] columns = new string[] { "DESCRIPCIÓN", "LUNES", "MARTES", "MIERCOLES", "JUEVES", "VIERNES", "SÁBADO", "TOTALES" };
                string start = startDate.ToString("dd-MMM-yyyy", new System.Globalization.CultureInfo("es-ES")),
                         end = endDate.ToString("dd-MMM-yyyy", new System.Globalization.CultureInfo("es-ES"));

                parameters = new SqlParameter[] { };
                List<Companies> companies = _unitOfWork.Repository<Companies>().GetSP<Companies>("[Finansii].[GetCompaniesNames]", parameters).ToList();
                string companyName = companies.Where(x => x.CompanyCode.Equals(companyCode)).FirstOrDefault().Name;

                parameters = new SqlParameter[]
                {
                    new SqlParameter("@EmpresaId", companyCode),
                };

                List<SalesAgent> salesAgents = _unitOfWork.Repository<SalesAgent>().GetSP<SalesAgent>("[Finansii].[GetSalesAgents]", parameters).ToList();

                EntityResponse response = this.GetHistoricalDetails(companyCode, salesAgent, expenseType, startDate, endDate/*, false*/).Result;
                if (response is EntityResponse<List<ExpenseDetail>> genericResponse)
                {
                    expenseDetails = genericResponse.Data;
                    expenseDetails = expenseDetails.FindAll(x => x.StatusId == 2);
                }
                else
                {
                    return EntityResponse.CreateError($"{response.Mensaje}");
                }

                ExpenseType type = _unitOfWorkGira.Repository<ExpenseType>().Query().Where(x => x.Id == expenseType && x.CompanyCode == companyCode).FirstOrDefault();

                var worksheet = workbook.Worksheets.Add("Reporte");

                worksheet.Range("C2:G2").Merge();
                worksheet.Cell("C2").Value = companyName;

                worksheet.Range("C4:G4").Merge();
                worksheet.Cell("C4").Value = $"Reporte de Gasto de Viaje";

                worksheet.Cell(6, 3).Value = "Nombre Completo";
                worksheet.Cell(7, 3).Value = "Desde";
                worksheet.Cell(8, 3).Value = "Hasta";

                worksheet.Range("E6:G6").Merge();
                worksheet.Cell(6, 5).Value = salesAgents.Find(x => x.PersonalCode == salesAgent).Name;
                worksheet.Range("E7:G7").Merge();
                worksheet.Cell(7, 5).Value = start;
                worksheet.Range("E8:G8").Merge();
                worksheet.Cell(8, 5).Value = end;

                row = firstRow;
                foreach (string c in columns)
                {
                    worksheet.Cell(row, column).Value = c;
                    worksheet.Cell(row, column).Style.Font.Bold = true;
                    worksheet.Cell(row, column).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, column).Style.Fill.BackgroundColor = XLColor.DarkBlue;

                    column++;
                }

                row++;
                var summary = BuildSummary(expenseDetails);

                foreach (var item in summary)
                {
                    worksheet.Cell(row, 2).Value = item.Description;
                    worksheet.Cell(row, 3).Value = item.Lunes == 0 ? "" : item.Lunes;
                    worksheet.Cell(row, 4).Value = item.Martes == 0 ? "" : item.Martes;
                    worksheet.Cell(row, 5).Value = item.Miercoles == 0 ? "" : item.Miercoles;
                    worksheet.Cell(row, 6).Value = item.Jueves == 0 ? "" : item.Jueves;
                    worksheet.Cell(row, 7).Value = item.Viernes == 0 ? "" : item.Viernes;
                    worksheet.Cell(row, 8).Value = item.Sabado == 0 ? "" : item.Sabado;
                    worksheet.Cell(row, 9).FormulaA1 = $"SUM(C{row}:H{row})";

                    worksheet.Cell(row, 2).Style.Font.Bold = true;
                    row++;
                }
                finalRow = row - 1;
                row++;
                worksheet.Range($"E{row}:H{row}").Merge();
                worksheet.Cell($"E{row}").Value = "SUBTOTAL";
                worksheet.Cell($"I{row}").FormulaA1 = $"SUM(I{firstRow}:I{finalRow})";

                row++;
                worksheet.Range($"E{row}:H{row}").Merge();
                worksheet.Cell($"E{row}").Value = "VALOR EXCEDIDO HOSPEDAJE";
                worksheet.Cell($"I{row}").FormulaA1 = $"";

                row++;
                worksheet.Range($"E{row}:H{row}").Merge();
                worksheet.Cell($"E{row}").Value = "TOTAL A PAGAR GASTOS DE VIAJE";
                worksheet.Cell($"I{row}").FormulaA1 = $"SUM(I{row - 2}:I{row - 1})";

                //Formato de encabezado de reporte
                dataRanges = worksheet.Ranges($"C2:G8, G{row - 3}:G{row}");
                dataRanges.Style.Font.Bold = true;

                //Formato para centralizar y colocar coma a todos los numeros
                dataRanges = worksheet.Ranges($"C2:G8,B{firstRow}:I{row}");
                dataRanges.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                dataRanges.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                dataRanges.Style.NumberFormat.Format = "#,##0.00";

                //Formato para agregar lineas a la tabla
                dataRange = worksheet.Range($"B{firstRow}:I{finalRow}");
                dataRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;

                //Formato para totales
                dataRange = worksheet.Range($"E{row - 2}:E{row}");
                dataRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                dataRange.Style.Font.Bold = true;

                worksheet.Columns("B:I").AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                var fileName = $"Reporte de {type.Name} del {start} al {end}";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return EntityResponse.CreateOk((content, contentType, fileName));
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }

        public static List<ExpenseSummaryRow> BuildSummary(List<ExpenseDetail> expenseDetails)
        {
            var result = expenseDetails
                .Where(x => x.ExpenseCategory != null)
                .Where(x => x.InvoiceDate.DayOfWeek != DayOfWeek.Sunday) // excluir domingo
                .GroupBy(x => x.ExpenseCategory.Name)
                .Select(g => new ExpenseSummaryRow
                {
                    Description = g.Key,

                    Lunes = (decimal)g
                        .Where(x => x.InvoiceDate.DayOfWeek == DayOfWeek.Monday)
                        .Sum(x => x.InvoiceAmount),

                    Martes = (decimal)g
                        .Where(x => x.InvoiceDate.DayOfWeek == DayOfWeek.Tuesday)
                        .Sum(x => x.InvoiceAmount),

                    Miercoles = (decimal)g
                        .Where(x => x.InvoiceDate.DayOfWeek == DayOfWeek.Wednesday)
                        .Sum(x => x.InvoiceAmount),

                    Jueves = (decimal)g
                        .Where(x => x.InvoiceDate.DayOfWeek == DayOfWeek.Thursday)
                        .Sum(x => x.InvoiceAmount),

                    Viernes = (decimal)g
                        .Where(x => x.InvoiceDate.DayOfWeek == DayOfWeek.Friday)
                        .Sum(x => x.InvoiceAmount),

                    Sabado = (decimal)g
                        .Where(x => x.InvoiceDate.DayOfWeek == DayOfWeek.Saturday)
                        .Sum(x => x.InvoiceAmount)
                })
                .OrderBy(x => x.Description)
                .ToList();

            return result;
        }

        public async Task<EntityResponse> GetImage(int id, string companyCode)
        {
            try
            {
                ExpenseDetail expenseDetail = _unitOfWorkGira.Repository<ExpenseDetail>().Query().Where(x => x.Id == id && x.CompanyCode == companyCode).FirstOrDefault();                

                if (String.IsNullOrEmpty(expenseDetail.ImagePath))
                {
                    return EntityResponse.CreateError("No se encontro una imagen para el gasto.");
                }

                /*var basePath = _evaConnectionSettings.Folder + "Gira/";
                var relativePath = expenseDetail.ImagePath.Replace("\\", "/").TrimStart('/');
                var fullPath = $"{basePath}{relativePath}";*/

                using var httpClient = new HttpClient();

                var request = new HttpRequestMessage(HttpMethod.Head, expenseDetail.ImagePath/*fullPath*/);
                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return EntityResponse.CreateError("Favor validar que la imagen se haya guardado correctamente.");

                return EntityResponse.CreateOk(expenseDetail.ImagePath);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }
    }
}