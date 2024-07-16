using PayWeb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.IO;
using System.Security.Cryptography;
using System.Text;
using OfficeOpenXml;

namespace CRM.Features.Accounting.HostToHostBanPais
{
    public class HostToHostBanPaisServices
    {
        public async Task<EntityResponse> GenerarEncryptarArchivo(string json)
        {
            // Llave de 128 bits (16 bytes)
            byte[] llave = Encoding.UTF8.GetBytes("0123456789ABCDEF");

            // Vector de inicialización (IV) de 16 bytes
            byte[] iv = new byte[16];
            byte[] datosCifrados;
            new Random().NextBytes(iv);
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Key = llave;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Crear un nuevo archivo de Excel
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");
                    worksheet.Cells["A1"].Value = "Hello";
                    worksheet.Cells["A2"].Value = "World";

                    // Guardar el archivo en memoria y cifrarlo
                    byte[] datos;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        excelPackage.SaveAs(ms);
                        datos = ms.ToArray();
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(datos, 0, datos.Length);
                            cs.FlushFinalBlock();
                        }

                        datosCifrados = ms.ToArray();

                        // Ruta específica donde se guardará el archivo cifrado
                        string rutaArchivoCifrado = @"C:\Users\bavila\OneDrive - INTERMODA SA DE CV\Desktop\Proyectos AX\50 - HostToHostBanPais\ArchivoCifrado.xlsx";
                        File.WriteAllBytes(rutaArchivoCifrado, datosCifrados);

                        Console.WriteLine("Archivo cifrado guardado en: " + rutaArchivoCifrado);
                    }
                }
            }
            return EntityResponse.CreateOk(datosCifrados);
        }
    }
}
