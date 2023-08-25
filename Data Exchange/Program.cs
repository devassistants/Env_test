using Data_Exchange.DataExchangeModels;
using LibDataExchange;
using LibDataExchange.Parameters;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Data_Exchange
{
    class Program
    {
        async static Task Main()
        {
            Console.WriteLine("Hello Enverus!");
            string downloadLink = "https://bakerhughesrigcount.gcs-web.com/static-files/7240366e-61cc-4acb-89bf-86dc1a0dffe8";
            string downloadExcelFilePath = "D:\\My Excel.xlsx";
            string saveCsvFilePath = "D:\\My Csv.csv";

            await Core.DownloadExcelAsync(downloadLink, downloadExcelFilePath);

            DeControllerBase<DeEntityTest> controller = new(2, 1);

            await controller.ReadExcelDataSourceAsync(downloadExcelFilePath);
            
            string result = "";
            DeEntityTest previousEntity = null;
            int numOfYears = 2;

            foreach (DeEntityTest entity in controller.EntityList.Where(x => x.HeaderValue != null && Convert.ToInt32(x.HeaderValue) > DateTime.Now.Year - numOfYears))
            {
                result = $"{result}{entity.GenerateCustomExportString(new EntityMethodParm(controller.SkippedFirstRows, controller.SkippedFirstColumns, previousEntity, null, null))}";
                previousEntity = entity;
            }

            using StreamWriter writer = new(saveCsvFilePath);
            writer.WriteLine(result);

            Console.WriteLine("All done, press any key to exit...");
            Console.Read();
        }
    }
}
