using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Data_Exchange
{
    internal static class Core
    {

        public async static Task DownloadExcelAsync(string downloadLink, string localFilePath)
        {
            if (File.Exists(localFilePath))
                File.Delete(localFilePath);

            using var client = new HttpClient();

            // NOTE: to save bandwidth, request compressed content
            client.DefaultRequestHeaders.AcceptEncoding.Clear();
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("all"));

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

    }
}
