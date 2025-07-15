using CRM.Common;
using CRM.Features.Credits.ReceiptBreakdown;
using CRM.Features.Credits.ReceiptDetailBreakdownReport;
using CRM.Infrastructure;
using CRM.Infrastructure.Core;
using IM_CDJournalSG;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PayWeb.Common;
using PayWeb.Infrastructure.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace CRM.Features.Accounting.CD
{
    public class CertificateDepositService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AXEndpoint _axEndpoint;
        private readonly ReceiptDetailBreakdownService _receiptDetailBreakdownService;

        public CertificateDepositService (IUnitOfWork unitOfWork, ReceiptDetailBreakdownService receiptDetailBreakdownService, AXEndpoint axEndpoint)
        {
            _unitOfWork = unitOfWork;
            _receiptDetailBreakdownService = receiptDetailBreakdownService;
            _axEndpoint = axEndpoint;
        }

        public async Task<EntityResponse> GetActiveCDBanks(string companyCode)
        {
            try
            {
                string year = DateTime.Today.Year.ToString();
                SqlParameter[] parameters = 
                {
                    new SqlParameter("@Year", year),
                    new SqlParameter("@CompanyCode", companyCode)
                };

                List<CDBank> banks = _unitOfWork.Repository<CDBank>().GetSP<CDBank>("[Finansii].[GetCDBanks]", parameters).ToList();

                return EntityResponse.CreateOk(banks);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }

        public async Task<EntityResponse> GetAllBanks(string companyCode)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@CompanyCode", companyCode)
                };

                List<Bank> banks = _unitOfWork.Repository<Bank>().GetSP<Bank>("[Finansii].[GetAllBanks]", parameters).ToList();

                return EntityResponse.CreateOk(banks);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en GetAllBanks: " + ex.Message);
            }
        }

        public decimal GetExchangeRate(string companyCode, DateTime date)
        {
            string exchangeRate = "0";

            try
            {
                string dateToString = date.ToString("yyyy-MM-dd", new System.Globalization.CultureInfo("es-ES"));

                SqlParameter[] parameters =
                {
                    new SqlParameter("@CompanyCode", companyCode),
                    new SqlParameter("@FromCurrency", "USD"),
                    new SqlParameter("@ToCurrency", "HNL"),
                    new SqlParameter("@Date", dateToString)
                };

                exchangeRate = _unitOfWork.StringRepository().GetSPForString("[Finansii].[GetExchangeRate_SP]", parameters).FirstOrDefault();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Math.Round(decimal.Parse(exchangeRate == null ? "0" : exchangeRate), 4);
        }

        public async Task<EntityResponse> GetCertificateWeeklyDetails(int certificateId)
        {
            try
            {
                SqlParameter[] parameters =
                {
                     new SqlParameter("@certificateId", certificateId)
                };

                List<WeeklyRecordsDto> records = _unitOfWork.Repository<WeeklyRecordsDto>().GetSP<WeeklyRecordsDto>("[Finansii].[GetWeeklyRecords]", parameters).ToList();

                foreach (WeeklyRecordsDto record in records)
                {
                    record.DatesRange = $"{record.StartDate.ToString("dd-MMM-yyyy", new System.Globalization.CultureInfo("es-ES"))} - {record.EndDate.ToString("dd-MMM-yyyy", new System.Globalization.CultureInfo("es-ES"))}";
                }

                return EntityResponse.CreateOk(records);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }

        public async Task<EntityResponse> GetCertificatesDeposit(string companyCode)
        {
            try
            {
                List<CertificateDeposit> certificates = _unitOfWork.Repository<CertificateDeposit>().Query().Where(x => x.CompanyCode == companyCode).ToList();
                return EntityResponse.CreateOk(certificates);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError("Error en método GetCertificatesDeposit: " + ex.Message);
            }
        }

        public async Task<EntityResponse> PostCertificateDeposit(CertificateDeposit newCertificateDeposit)
        {
            try
            {
                List<CertificateDeposit> certificates = new();
                EntityResponse entityResponse = new();

                decimal rateInDecimal = newCertificateDeposit.RatePercentage / 100;
                decimal dailyIncome = (newCertificateDeposit.Amount * rateInDecimal) / 360;

                newCertificateDeposit.CDNumber = newCertificateDeposit.CDNumber.Replace(" ", "");
                newCertificateDeposit.DailyIncome = dailyIncome;
                newCertificateDeposit.isEnabled = true;

                EntityResponse response = GetCertificatesDeposit(newCertificateDeposit.CompanyCode).Result;

                if (response is EntityResponse<List<CertificateDeposit>> genericResponse)
                {
                    certificates = genericResponse.Data;
                }
                else
                {
                    return EntityResponse.CreateError($"{response.Mensaje}");
                }

                if(newCertificateDeposit.RenovationCertificate == null)
                {
                    if (certificates.Exists(x => x.CompanyCode == newCertificateDeposit.CompanyCode && x.isEnabled == true && x.Bank == newCertificateDeposit.Bank
                                          && x.Currency == newCertificateDeposit.Currency && x.CDNumber.Replace(" ", "") == newCertificateDeposit.CDNumber.Replace(" ", "")
                                          && x.Id != newCertificateDeposit.Id))
                    {
                        return EntityResponse.CreateError($"Ya existe un certificado para ese banco con el mismo número de CD");
                    }
                }

                if (newCertificateDeposit.RenovationCertificate != null && newCertificateDeposit.Id == 0)
                {
                    CertificateDeposit certificate = certificates.Where(x => x.Bank == newCertificateDeposit.Bank && x.CDNumber == newCertificateDeposit.RenovationCertificate).FirstOrDefault();
                    certificate.isEnabled = false;

                    _unitOfWork.Repository<CertificateDeposit>().Update(certificate);
                    await _unitOfWork.SaveChangesAsync();
                }

                if(newCertificateDeposit.Id != 0)
                {
                    CertificateDeposit certificate = _unitOfWork.Repository<CertificateDeposit>().Query().Where(x => x.CompanyCode == newCertificateDeposit.CompanyCode
                                                                                                                           && x.Id == newCertificateDeposit.Id).FirstOrDefault();

                    certificate.CDNumber = newCertificateDeposit.CDNumber.Replace(" ", "");
                    certificate.StartDate = newCertificateDeposit.StartDate;
                    certificate.EndDate = newCertificateDeposit.EndDate;
                    certificate.Amount = newCertificateDeposit.Amount;
                    certificate.RatePercentage = newCertificateDeposit.RatePercentage;
                    certificate.Comment = newCertificateDeposit.Comment;
                    certificate.DailyIncome = dailyIncome;
                    certificate.isCapitalizable = newCertificateDeposit.isCapitalizable;
                    certificate.ModificationDate = DateTime.Now;

                    _unitOfWork.Repository<CertificateDeposit>().Update(certificate);
                    await _unitOfWork.SaveChangesAsync();
                    return EntityResponse.CreateOk(certificate);
                }

                _unitOfWork.Repository<CertificateDeposit>().Add(newCertificateDeposit);
                await _unitOfWork.SaveChangesAsync();
                return EntityResponse.CreateOk(newCertificateDeposit);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }

        public async Task<EntityResponse> DeleteCertificate(int id)
        {
            try
            {
                List<WeeklyRecord> weeklyRecords = _unitOfWork.Repository<WeeklyRecord>().Query().Where(x => x.CertificateId == id).ToList();

                if (weeklyRecords.Count <= 0)
                {
                    CertificateDeposit certificate = _unitOfWork.Repository<CertificateDeposit>().Query().Where(x => x.Id == id).FirstOrDefault();
                   
                    _unitOfWork.Repository<CertificateDeposit>().Delete(certificate);
                    await _unitOfWork.SaveChangesAsync();
                    return EntityResponse.CreateOk();
                }

                return EntityResponse.CreateError("No se pudo eliminar el certificado ya cuenta con diarios creados.");

            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }

        public EntityResponse downloadExcel(string companyCode)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                SqlParameter[] parameters = { };
                string column = "A";
                int row = 1, startSumRow = 0, endSumRow = 0;

                //Columns
                string[] headers = new string[]
                {
                        "Fecha", "Vence", "N. CD", "Moneda", "Valor", "Tasa", "Intereses Diarios"
                };

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Reporte");

                    List<WeeklyRecord> allWeeklyRecords = _unitOfWork.Repository<WeeklyRecord>().Query().ToList();
                    int firstWeek = allWeeklyRecords.OrderBy(x => x.Week).First().Week;
                    int lastWeek = allWeeklyRecords.OrderByDescending(x => x.Week).First().Week;
                    int week = firstWeek;

                    foreach (string header in headers)
                    {
                        worksheet.Cells[$"{column}{row}"].Value = header;
                        worksheet.Cells[$"{column}{row}"].Style.Font.Bold = true;
                        column = GetNextColumn(column);
                    }

                    do
                    {
                        worksheet.Cells[$"{column}{row}"].Value = "Semana " + week;
                        worksheet.Cells[$"{column}{row}"].Style.Font.Bold = true;

                        DateTime date = GetLastDayOfWeek(week.ToString());

                        row++;
                        worksheet.Cells[$"{column}{row}"].Value = date.ToString("dd-MMM-yyyy", new System.Globalization.CultureInfo("es-ES"));
                        
                        row++;
                        date = date.DayOfWeek == DayOfWeek.Saturday ? date.AddDays(-1) :
                               date.DayOfWeek == DayOfWeek.Sunday ? date.AddDays(-2) :
                               date;

                        worksheet.Cells[$"{column}{row}"].Value = GetExchangeRate(companyCode, date);

                        week++;
                        column = GetNextColumn(column);
                        row -= 2;
                    } while (week <= lastWeek);

                    row = 2;
                    column = "A";

                    parameters = new SqlParameter[]
                    {
                        new SqlParameter("@CompanyCode", companyCode)
                    };

                    List<CDBankName> cdBankList = _unitOfWork.Repository<CDBankName>().GetSP<CDBankName>("[Finansii].[GetCDBankName]", parameters).ToList();
                    List<string> bankNames = cdBankList.GroupBy(x => x.Name).Select(x => x.Key).ToList();

                    foreach (string bankName in bankNames)
                    {
                        column = "A";
                        row++;
                        worksheet.Cells[$"{column}{row}"].Value = bankName;
                        worksheet.Cells[$"{column}{row}"].Style.Font.Bold = true;
                        worksheet.Cells[$"{column}{row}"].Style.Font.UnderLine = true;
                        row++;

                        foreach (string header in headers)
                        {
                            worksheet.Cells[$"{column}{row}"].Value = header;
                            worksheet.Cells[$"{column}{row}"].Style.Font.Bold = true;

                            worksheet.Cells[$"{column}{row}"].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells[$"{column}{row}"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells[$"{column}{row}"].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            worksheet.Cells[$"{column}{row}"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            column = GetNextColumn(column);
                        }
                        row++;
                        startSumRow = row;

                        int cdsQtys = 0;

                        foreach (CDBankName cDBankName in cdBankList.Where(x => x.Name == bankName).OrderBy(x => x.Id))
                        {
                            cdsQtys++;
                            CertificateDeposit certificate = _unitOfWork.Repository<CertificateDeposit>().Query().Where(x => x.Id == cDBankName.Id).FirstOrDefault();
                            List<WeeklyRecord> weeklyRecords = allWeeklyRecords.Where(x => x.CertificateId == cDBankName.Id).ToList();
                                                        
                            column = "A";

                            worksheet.Cells[$"{column}{row}"].Value = certificate.StartDate.ToString("dd-MMM-yyyy", new System.Globalization.CultureInfo("es-ES"));
                            worksheet.Cells[$"{column}{row}"].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            column = GetNextColumn(column);

                            worksheet.Cells[$"{column}{row}"].Value = certificate.EndDate.ToString("dd-MMM-yyyy", new System.Globalization.CultureInfo("es-ES"));
                            column = GetNextColumn(column);

                            worksheet.Cells[$"{column}{row}"].Value = certificate.CDNumber;
                            column = GetNextColumn(column);

                            worksheet.Cells[$"{column}{row}"].Value = certificate.Currency;
                            column = GetNextColumn(column);

                            worksheet.Cells[$"{column}{row}"].Value = certificate.Amount;
                            worksheet.Cells[$"{column}{row}"].Style.Numberformat.Format = "#,##0.00";
                            column = GetNextColumn(column);

                            worksheet.Cells[$"{column}{row}"].Value = certificate.RatePercentage;
                            worksheet.Cells[$"{column}{row}"].Style.Numberformat.Format = "#0\\.00%";
                            column = GetNextColumn(column);

                            worksheet.Cells[$"{column}{row}"].Value = certificate.DailyIncome;
                            worksheet.Cells[$"{column}{row}"].Style.Numberformat.Format = "#,##0.00";
                            worksheet.Cells[$"{column}{row}"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            column = GetNextColumn(column);

                            List<WeeklyRecord> records = _unitOfWork.Repository<WeeklyRecord>().Query().Where(x => x.CertificateId == certificate.Id).ToList();

                            foreach (WeeklyRecord record in records)
                            {
                                int weeksDifference = record.Week - firstWeek;

                                column = AddStepsToColumn(8/*Es la letra H*/, weeksDifference);
                                worksheet.Cells[$"{column}{row}"].Value = record.Amount; 
                                worksheet.Cells[$"{column}{row}"].Style.Numberformat.Format = "#,##0.00";
                            }

                            endSumRow = row;
                            row++;
                        }

                        week = firstWeek;
                        column = "H";

                        do
                        {
                            if(startSumRow != endSumRow)
                            {
                                worksheet.Cells[$"{column}{endSumRow + 1}"].Formula = $"SUM({column}{startSumRow}:{column}{endSumRow})";
                                worksheet.Cells[$"{column}{endSumRow + 1}"].Style.Numberformat.Format = "#,##0.00";
                                column = GetNextColumn(column);

                                week++;
                            }
                            else
                            {
                                week = lastWeek + 1;
                            }

                        } while (week <= lastWeek);

                        worksheet.Cells[$"G{row}"].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        var range = worksheet.Cells[$"A{row}:G{row}"];

                        // Apply borders to the range
                        var border = range.Style.Border;
                        border.Bottom.Style = ExcelBorderStyle.Thin;

                        worksheet.Cells[$"E{row}"].Formula = $"SUM(E{startSumRow}:E{endSumRow})";
                        worksheet.Cells[$"E{row}"].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[$"E{row}"].Style.Font.Bold = true;
                        worksheet.Cells[$"E{row}"].Style.Font.UnderLine = true;
                        worksheet.Cells[$"E{row}"].Style.Font.Italic = true;
                        worksheet.Calculate();
                        row++;
                    }
                    worksheet.View.FreezePanes(2, 1);
                    worksheet.Cells["A:AZ"].AutoFitColumns();

                    /*string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string filePath = Path.Combine(desktopPath, "MyExcelFile.xlsx");
                    package.SaveAs(new FileInfo(filePath));*/

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

        public DateTime GetLastDayOfWeek(string week)
        {
            DateTime date = new();
            EntityResponse response = new();
            List<FiscalYear> years = new();
            List<FiscalWeek> weeks = new();

            response = _receiptDetailBreakdownService.GetFiscalYears().Result;

            if (response is EntityResponse<List<FiscalYear>> genericResponse)
            {
                years = genericResponse.Data;
            }

            response = _receiptDetailBreakdownService.GetFiscalWeeks(years.Find(x => x.Year == DateTime.Now.Year).RecId).Result;

            if (response is EntityResponse<List<FiscalWeek>> genericResponse2)
            {
                weeks = genericResponse2.Data;
            }

            date = weeks.Find(x => x.Week == week).EndDate;
            return date;
        } 

        public async Task<EntityResponse> PostWeeklyJournal(string companyCode, string fiscalYearRecId, string week)
        {
            try
            {
                EntityResponse response = new();
                SqlParameter[] parameters = { };
                List<string> journals = new();
                List<string> errors = new();
                string responseMessage = "", journal = "";

                List<int> certificatesWithRecords = _unitOfWork.Repository<WeeklyRecord>().Query().Where(x => x.Week == int.Parse(week)).Select(x => x.CertificateId).ToList();
                List<CertificateDeposit> certificateDeposits = _unitOfWork.Repository<CertificateDeposit>().Query().Where(x => x.isEnabled == true && !certificatesWithRecords.Contains(x.Id)).ToList();

                /*Commented by spineda on may/31/2025 - Begin*/
                List<string> currencies = certificateDeposits.Select(x => x.Currency).Distinct().ToList();

                foreach(string currency in currencies)
                {
                    List<CDLINES> LIST = new();
                    parameters = new SqlParameter[]
                    {
                        new SqlParameter("@CompanyCode", companyCode),
                        new SqlParameter("@FiscalCalendarYear", fiscalYearRecId),
                        new SqlParameter("@SelectedWeek", week),
                        new SqlParameter("@CurrencyCode", currency),
                    };

                    List<Certificate> certificateList = _unitOfWork.Repository<Certificate>().GetSP<Certificate>("[Finansii].[GetCDJournalLines]", parameters).ToList();
                    certificateList.ForEach(element =>
                    {
                        CDLINES LINE = new CDLINES();
                        LINE.CERTIFICATENUMBER = "";
                        LINE.LEDGERDIMENSION = element.LEDGERDIMENSION;
                        LINE.TRANSDATE = element.TRANSDATE;
                        LINE.JOURNALDATE = element.JOURNALDATE;
                        LINE.LEDGERJOURNALTRANSTXT = element.LEDGERJOURNALTRANSTXT;
                        LINE.CURRENCYCODE = element.CURRENCYCODE;
                        LINE.AMOUNTCURDEBIT = element.AMOUNTCURDEBIT;
                        LINE.AMOUNTCURCREDIT = element.AMOUNTCURCREDIT;
                        LINE.OFFSETLEDGERDIMENSION = "";
                        LIST.Add(LINE);
                    });

                    if (LIST.Count <= 1)
                    {
                        errors.Add($"Error en Creación de Diario {currency}: Ya se registraron todos los certificados en la semana seleccionada.");
                    }

                    if (LIST.Sum(x => x.AMOUNTCURDEBIT) <= 0)
                    {
                        errors.Add($"Error en Creación de Diario {currency}: No se pudo calcular el total de intereses.");
                    }

                    response = CallService(LIST, companyCode).Result;

                    if (response is EntityResponse<string> genericResponse)
                    {
                        if (!genericResponse.Data.Contains("LD"))
                        {
                            errors.Add($"Error en Creación de Diario {currency}: {genericResponse.Data}");;
                        }
                        journal = genericResponse.Data;
                        journals.Add(journal);
                    }

                    foreach (Certificate certificate in certificateList.FindAll(x => x.ID != 0))
                    {
                        WeeklyRecord weeklyRecord = new()
                        {
                            CertificateId = certificate.ID,
                            AmountInCurrency = certificate.AMOUNTINCURRENCY,
                            Amount = certificate.AMOUNTCURDEBIT,
                            Week = int.Parse(week),
                            Journal = journal
                        };

                        _unitOfWork.Repository<WeeklyRecord>().Add(weeklyRecord);
                        await _unitOfWork.SaveChangesAsync();
                    }

                }
                /*Commented by spineda on may/31/2025 - End*/

                responseMessage = journals.Count > 0 ? $"Se crearón los siguientes diarios: {string.Join(", ", journals)}. " : "";

                if (errors.Count > 0)
                {
                    responseMessage += $"Se generarón los siguientes errores: {string.Join(", ", errors)}";
                }

                return EntityResponse.CreateOk(responseMessage);
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }

        public async Task<EntityResponse> PostFinalJournal(string companyCode, int certificateId)
        {
            try
            {
                List<CDLINES> LIST = new();
                EntityResponse response = new();
                string journal = "";
                SqlParameter[] parameters = { };

                string todaysDate = DateTime.Now.Year.ToString() + "-" + DateTime.Now.ToString("MM") + "-" + DateTime.Now.ToString("dd");

                parameters = new SqlParameter[]
                {
                    new SqlParameter("@CertificateId", certificateId),
                    new SqlParameter("@CompanyCode", companyCode),
                    new SqlParameter("@Date", todaysDate)
                };

                LIST = _unitOfWork.Repository<CDLINES>().GetSP<CDLINES>("[Finansii].[GetTotalSumOfInterest]", parameters).ToList();

                if (LIST.Sum(x => x.AMOUNTCURCREDIT) <= 0)
                {
                    return EntityResponse.CreateError("No se pudo calcular el total de intereses.");
                }

               response = CallService(LIST, companyCode).Result;

                if (response is EntityResponse<string> genericResponse)
                {
                    if (!genericResponse.Data.Contains("LD"))
                    {
                        return EntityResponse.CreateError(genericResponse.Data);
                    }

                    journal = genericResponse.Data;
                }

                parameters = new SqlParameter[]
                {
                    new SqlParameter("@CertificateId", certificateId),
                    new SqlParameter("@CompanyCode", companyCode),
                    new SqlParameter("@Date", todaysDate)
                };

                LIST = _unitOfWork.Repository<CDLINES>().GetSP<CDLINES>("[Finansii].[GetTotalOfCD]", parameters).ToList();

                if (LIST.Sum(x => x.AMOUNTCURDEBIT) <= 0)
                {
                    return EntityResponse.CreateError("No se pudo obtener el total del CD.");
                }
                //Cuando se cree el diario final, verificar si esta habilitada la opcion de registrar.
                response = CallService(LIST, companyCode).Result;

                if (response is EntityResponse<string> genericResponse2)
                {
                    if (!genericResponse2.Data.Contains("LD"))
                    {
                        return EntityResponse.CreateError(genericResponse2.Data);
                    }

                    journal += (" y " + genericResponse2.Data);
                }

                CertificateDeposit certificate = _unitOfWork.Repository<CertificateDeposit>().Query().Where(x => x.Id == certificateId).FirstOrDefault();
                certificate.isEnabled = false;

                _unitOfWork.Repository<CertificateDeposit>().Update(certificate);
                await _unitOfWork.SaveChangesAsync();

                return EntityResponse.CreateOk($"Se finalizó el certificado y se crearón los diarios: {journal}");
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }

        public async Task<EntityResponse> CallService(List<CDLINES> LIST, string companyCode)
        {
            CDHEADER HEADER = new();

            try
            {
                HEADER.LINES = LIST.ToArray();

                string lines = HEADER.Serialize();
                IM_CDJournalSG.CallContext context = new() { Company = companyCode };
                var serviceClient = new M_CDJournalClient(_axEndpoint.GetBinding(), _axEndpoint.GetEndpointAddr("IM_CDJournalSG"));

                //serviceClient.ClientCredentials.Windows.ClientCredential.UserName = "servicio_ax";
                //serviceClient.ClientCredentials.Windows.ClientCredential.Password = "Int3r-M0d@.aX$3Rv";

                serviceClient = (M_CDJournalClient)_axEndpoint.Service(serviceClient);

                string dataValidation = string.Format("<INTEGRATION><COMPANY><CODE>{0}</CODE><USER>{1}</USER></COMPANY></INTEGRATION>", context.Company, "servicio_ax");
                IM_CDJournalInitRequest request = new IM_CDJournalInitRequest();
                request.CallContext = context;
                request._dataValidationXML = dataValidation;
                request._lineXML = lines;
                var resp = await serviceClient.initAsync(context, dataValidation, lines);

                return EntityResponse.CreateOk(resp.response.ToString());
            }
            catch (Exception ex)
            {
                return EntityResponse.CreateError(ex.Message);
            }
        }

        /*private NetTcpBinding GetBinding()
        {
            var netTcpBinding = new NetTcpBinding();
            netTcpBinding.Name = "NetTcpBinding_IM_WMSCreateJournalServices";
            netTcpBinding.MaxBufferSize = int.MaxValue;
            netTcpBinding.MaxReceivedMessageSize = int.MaxValue;
            return netTcpBinding;
        }

        private EndpointAddress GetEndpointAddr()
        {
            //string url = "net.tcp://gim-pro3-AOS:8201/DynamicsAx/Services/IM_CDJournalSG";
            string url = "net.tcp://gim-dev-aos:8201/DynamicsAx/Services/IM_CDJournalSG"; 
            string user = "sqladmin@intermoda.com.hn";

            var uri = new Uri(url);
            //var epid = new UpnEndpointIdentity(user);
            var addrHdrs = new AddressHeader[0];
            var endpointAddr = new EndpointAddress(uri, addrHdrs); //, epid, addrHdrs);
            return endpointAddr;
        }*/

        public static string GetNextColumn(string currentColumn)
        {
            char[] columnLetters = currentColumn.ToCharArray();
            int i = columnLetters.Length - 1;

            // Traverse from the end of the string to handle overflow
            while (i >= 0)
            {
                if (columnLetters[i] == 'Z')
                {
                    columnLetters[i] = 'A'; // Reset to 'A'
                    i--;                   // Move to the next position
                }
                else
                {
                    columnLetters[i]++;    // Increment the current character
                    return new string(columnLetters); // Return the updated column
                }
            }

            // If we reach here, prepend one incremented letter
            return new string('A', columnLetters.Length + 1);
        }

        public static string AddStepsToColumn(int columnIndex, int steps)
        {
            columnIndex += steps;                               // Add the steps
            return ColumnNumberToName(columnIndex);             // Convert back to column name
        }

        private static string ColumnNumberToName(int columnNumber)
        {
            string columnName = string.Empty;
            while (columnNumber > 0)
            {
                int remainder = (columnNumber - 1) % 26;
                columnName = (char)(remainder + 'A') + columnName;
                columnNumber = (columnNumber - 1) / 26;
            }
            return columnName;
        }

    }
}
