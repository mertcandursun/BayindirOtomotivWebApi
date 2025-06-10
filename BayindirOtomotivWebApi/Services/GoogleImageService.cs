using System.Text.Json;

namespace BayindirOtomotivWebApi.Services
{
    public class GoogleImageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _cx;

        public GoogleImageService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            // appsettings.json -> GoogleSearch:ApiKey, GoogleSearch:Cx
            _apiKey = config["GoogleSearch:ApiKey"];
            _cx = config["GoogleSearch:Cx"];

            _httpClient = httpClientFactory.CreateClient("GoogleCustomSearchClient");
        }

        /// <summary>
        /// Görseli Google Custom Search ile arar, ilk sonucu indirir ve base64 döndürür.
        /// Eğer sonuç yok veya hata varsa null döner.
        /// </summary>
        public async Task<string> DownloadImageAsBase64Async(string query)
        {
            try
            {
                // 1) Google Custom Search Image API
                var url = $"https://www.googleapis.com/customsearch/v1?key={_apiKey}&cx={_cx}"
                        + $"&q={Uri.EscapeDataString(query)}&searchType=image&num=1";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);

                // items[0].link
                if (!doc.RootElement.TryGetProperty("items", out var itemsJson))
                    return null;
                if (itemsJson.GetArrayLength() == 0)
                    return null;

                var firstItem = itemsJson[0];
                var imageLink = firstItem.GetProperty("link").GetString();
                if (string.IsNullOrEmpty(imageLink))
                    return null;

                // 2) Görseli indirip base64'e çevir
                var imageBytes = await _httpClient.GetByteArrayAsync(imageLink);
                var base64 = Convert.ToBase64String(imageBytes);

                return base64;
            }
            catch
            {
                return null;
            }
        }
    }
}
