using ChoETL;
using ClosedXML.Excel;
using MiniExcelLibs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyTests
{
    class Program
    {

        public static void DownloadExcel(string downloadLink, string localFilePath)
        {
            if (File.Exists(localFilePath))
                File.Delete(localFilePath);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadLink);

            request.AutomaticDecompression = DecompressionMethods.All;
            using var response = request.GetResponse();
            using var stream = response.GetResponseStream();
            using var output = File.Create(localFilePath);
            stream.CopyTo(output);
        }

        public async static Task DownloadExcelAsync(string downloadLink, string localFilePath)
        {
            if (File.Exists(localFilePath))
                File.Delete(localFilePath);

            using var client = new HttpClient();

            // NOTE: to save bandwidth, request compressed content
            client.DefaultRequestHeaders.AcceptEncoding.Clear();
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("all"));
            //client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            //client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            //client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("br"));

            //// NOTE: accept all languages
            //client.DefaultRequestHeaders.AcceptLanguage.Clear();
            //client.DefaultRequestHeaders.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("*"));

            // NOTE: accept all media types
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/xlsx"));

            using var result = await client.GetAsync(downloadLink);

            if (result.IsSuccessStatusCode)
            {
                var a = await result.Content.ReadAsByteArrayAsync();

                File.WriteAllBytes(localFilePath, a);
            }
        }

        public static void ReadExcelMini(string localFilePath)
        {
            using var reader = MiniExcel.GetReader(localFilePath, false);
            List<List<object>> values = new();

            while (reader.Read())
            {
                List<object> row = new();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetValue(i));
                }
                values.Add(row);
            }
        }

        public static void ReadExcelCloseXml(string localFilePath)
        {
            using var workbook = new XLWorkbook(localFilePath);
            var worksheet = workbook.Worksheet(1);

            var rows = worksheet.RangeUsed().RowsUsed(XLCellsUsedOptions.All).Skip(2); // Skip header row
            foreach (var row in rows)
            {
                //var rowNumber = row.RowNumber();
            }
        }

        public static void SaveCsv(string localFilePath)
        {
            List<MyStructure> structures = new()
            {
                new MyStructure() { MyName = "St1", MyValue = 1 },
                new MyStructure() { MyName = null, MyValue = null },
                new MyStructure() { MyName = "St2", MyValue = 2 }
            };

            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            string jsonString = JsonSerializer.Serialize(structures, serializeOptions);
            ChoCSVRecordConfiguration configuration = new()
            { 
                Delimiter = ",", 
                FileHeaderConfiguration = new ChoCSVFileHeaderConfiguration() { HasHeaderRecord = false } 
            };
            using var r = ChoJSONReader.LoadText(jsonString);
            using var w = new ChoCSVWriter(localFilePath, configuration);

            w.Write(r);
        }

        async static Task Main()
        {
            Console.WriteLine("Hello World!");
            string downloadLink = "https://bakerhughesrigcount.gcs-web.com/static-files/7240366e-61cc-4acb-89bf-86dc1a0dffe8";
            string localFilePath = "D:\\My Excel.xlsx";

            DownloadExcel(downloadLink, localFilePath);
            localFilePath = "D:\\My Excel Async.xlsx";
            await DownloadExcelAsync(downloadLink, localFilePath);

            ReadExcelMini(localFilePath);
            ReadExcelCloseXml(localFilePath);

            SaveCsv("D:\\My Csv.csv");

            Console.WriteLine("Done!!!");
        }

    }
}
