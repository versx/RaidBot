namespace T.Extensions
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    public static class HttpClientExtensions
    {
        public static async Task DownloadAsync(this HttpClient httpClient, Uri requestUri, string fileName)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                using (
                    Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
                    stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                {
                    await contentStream.CopyToAsync(stream);
                }
            }
        }

        public static async Task<bool> IsImageUrlAsync(this HttpClient httpClient, string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Head, url))
            {
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.Headers.ContentType.MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
                }
                return false;
            }
        }
    }
}