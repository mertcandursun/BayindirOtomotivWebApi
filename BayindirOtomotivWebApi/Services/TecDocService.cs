using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using BayindirOtomotivWebApi.Models; // DTO'larınızın bulunduğu namespace

namespace BayindirOtomotivWebApi.Services
{
    public class TecDocService
    {
        private readonly HttpClient _client;
        private readonly string _rapidApiHost;
        private readonly string _rapidApiKey;

        public TecDocService(IHttpClientFactory httpClientFactory)
        {
            // "TecDocClient" ismini Program.cs’de HttpClient ayarlarınızda tanımladığınızı varsayıyoruz.
            _client = httpClientFactory.CreateClient("TecDocClient");

            // Bu değerleri isterseniz appsettings.json’dan da çekebilirsiniz.
            _rapidApiHost = "tecdoc-catalog.p.rapidapi.com";
            _rapidApiKey = "b6d48a76c1msh6716422ba0e5a66p163b11jsn95c156a37e0a";
        }

        /// <summary>
        /// Belirtilen OEM numarası ve dil ID'sine göre TecDoc Catalog API'den arama yapar.
        /// Örnek URL: 
        /// https://tecdoc-catalog.p.rapidapi.com/articles-oem/search/lang-id/4/article-oem-search-no/8F0513035N
        /// </summary>
        /// <param name="articleOEM">Aranacak OEM numarası</param>
        /// <param name="langId">Dil ID'si (varsayılan: 4)</param>
        /// <returns>TecDoc arama sonuçlarını içeren DTO</returns>
        public async Task<TecDocArticleOEMSearchResponse> SearchArticleOEMAsync(string articleOEM, int langId = 4)
        {
            // URL oluşturma
            var url = $"https://{_rapidApiHost}/articles-oem/search/lang-id/{langId}/article-oem-search-no/{articleOEM}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-rapidapi-host", _rapidApiHost);
            request.Headers.Add("x-rapidapi-key", _rapidApiKey);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // API yanıtı doğrudan bir dizi şeklinde, dolayısıyla listeye deserialize ediyoruz
            var articles = JsonSerializer.Deserialize<List<TecDocArticleOEMDto>>(json, options);

            // Sonucu sarmalayarak döndürün
            return new TecDocArticleOEMSearchResponse { articles = articles };
        }


        public async Task<string> DownloadImageAsBase64FromUrl(string imageUrl)
        {
            var response = await _client.GetAsync(imageUrl);
            if (!response.IsSuccessStatusCode)
                return null;

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return Convert.ToBase64String(bytes);
        }

    }
}
