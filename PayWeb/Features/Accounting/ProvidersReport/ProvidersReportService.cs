using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using PayWeb.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace CRM.Features.Accounting.ProvidersReport
{
    public class ProvidersReportService
    {
        public async Task<EntityResponse> getProvidersReport(IFormFile file)
        {
            var list = new List<ProviderReportDto>();
            Boolean next = true;
            Boolean dueDate = false;

            try
            {
                if (file == null || file.Length == 0)
                    return EntityResponse.CreateError("El archivo esta vacio");

                using(var memoryStream = new MemoryStream()) 
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    ExcelWorksheet worksheet;
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (ExcelPackage package = new ExcelPackage(memoryStream))
                    {
                        worksheet = package.Workbook.Worksheets[0];
                        var row = 1;
                        var tmp = new ProviderReportDto();

                        do
                        {
                            var value = worksheet.Cells[row, 3].Value;
                            string cellvalue = "";

                            try
                            {
                                cellvalue = value == null ? "" : value.ToString(); 
                            }
                            catch (Exception ex)
                            {
                                return EntityResponse.CreateError(ex.Message);
                            }

                            if (dueDate && cellvalue != "Total")
                            {
                                var cell = (string)worksheet.Cells[row, 4].Value;

                                var valor = (string)worksheet.Cells[row, 5].Value;
                                valor = valor.Replace(".", "");
                                valor = valor.Replace(",", ".");

                                if (cell.StartsWith("AA-"))
                                {
                                    tmp.Advance += Convert.ToDecimal(valor);
                                }
                                else
                                {
                                    tmp.RealBalance += Convert.ToDecimal(valor);
                                }
                            }
                            else if (cellvalue == "Fecha de vencimiento")
                            {
                                dueDate = true;
                            }
                            else if (cellvalue != null && cellvalue.StartsWith("PRV-"))
                            {
                                tmp.Provider = cellvalue;
                                tmp.Name = (string)worksheet.Cells[row, 4].Value;
                                tmp.Group = (string)worksheet.Cells[row, 5].Value;
                                var t = (double)worksheet.Cells[row + 3, 5].Value;
                                DateTime baseDate = new DateTime(1899, 12, 30);

                                tmp.Date = baseDate.AddDays(t);
                                tmp.RealBalance = 0;
                                tmp.Advance = 0;
                            }
                            else if (cellvalue == "Total")
                            {
                                list.Add(tmp);
                                tmp = new ProviderReportDto();
                                dueDate = false;
                            }
                            else if (cellvalue == "Total general")
                            {
                                next = false;
                            }
                            row++;
                        } while (next);
                    }
                }

                return EntityResponse.CreateOk(list);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }

        public EntityResponse downloadProvidersReport(List<ProviderReportDto> providers)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            EntityResponse response = new();
            int firstRow = 5, row;
            char firstColumn = 'B', column;

            column = firstColumn;
            row = firstRow;

            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Reporte");

                    //Title
                    worksheet.Cells[$"D1"].Value = "INTERMODA";
                    worksheet.Cells[$"D1"].Style.Font.Size = 14;
                    worksheet.Cells[$"D1"].Style.Font.Bold = true;
                    worksheet.Cells[$"D1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[$"D2"].Value = "Reporte de Antigüedad Proveedores";
                    worksheet.Cells[$"D2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[$"D3"].Value = $"Al {providers[0].Date.Day} de {providers[0].Date.ToString("MMMM", new CultureInfo("es-Es"))} del {providers[0].Date.Year}";
                    worksheet.Cells[$"D3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;


                    //Columns
                    string[] headers = new string[]
                    {
                        "Fecha", "Proveedor", "Nombre", "Grupo", "Saldo Real", "Anticipos"
                    };

                    foreach (string header in headers)
                    {
                        worksheet.Cells[$"{column}{row}"].Value = header;
                        column++;
                    }

                    foreach(ProviderReportDto provider in providers)
                    {
                        column = firstColumn;
                        row++;

                        worksheet.Cells[$"{column}{row}"].Value = provider.Date;
                        worksheet.Cells[$"{column}{row}"].Style.Numberformat.Format = "dd-mm-yyyy";
                        column++;
                        worksheet.Cells[$"{column}{row}"].Value = provider.Provider;
                        column++;
                        worksheet.Cells[$"{column}{row}"].Value = provider.Name;
                        column++;
                        worksheet.Cells[$"{column}{row}"].Value = provider.Group;
                        column++;
                        worksheet.Cells[$"{column}{row}"].Value = provider.RealBalance;
                        worksheet.Cells[$"{column}{row}"].Style.Numberformat.Format = "#,##0.00";
                        column++;
                        worksheet.Cells[$"{column}{row}"].Value = provider.Advance;
                        worksheet.Cells[$"{column}{row}"].Style.Numberformat.Format = "#,##0.00";
                    }

                    string range = $"{firstColumn}{firstRow}:{column}{row}";

                    worksheet.Cells[range].AutoFitColumns();

                    ExcelRange totalCell;
                    var tableRange = worksheet.Cells[range];
                    var table = worksheet.Tables.Add(tableRange, "MyTable");

                    table.ShowTotal = true;
                    row++;

                    table.Columns[5].TotalsRowFunction = RowFunctions.Sum;
                    totalCell = tableRange[$"F{row}"];
                    totalCell.Style.Numberformat.Format = "#,##0.00";

                    table.Columns[4].TotalsRowFunction = RowFunctions.Sum;
                    totalCell = tableRange[$"G{row}"];
                    totalCell.Style.Numberformat.Format = "#,##0.00";

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    return EntityResponse.CreateOk(stream);
                }

            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }
    }
}