using BayindirOtomotivWebApi.Models.Basbug;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BayindirOtomotivWebApi.Services
{
    public class BasbugService
    {
        private readonly HttpClient _client;
        private string _token;

        public BasbugService(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("BasbugClient");
        }

        public async Task AuthenticateAsync()
        {
            var loginBody = new
            {
                KullaniciAdi = "MS8112",
                Parola = "UMcB1QCc4owQdafk",
                ClientSecret = "W2wOU8V6w3eSWfo6sxi2CThf1g9EYmZ3",
                ClientID = "materialApi"
            };

            var response = await _client.PostAsJsonAsync("/auth/Login", loginBody);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("token", out var tokenElement))
            {
                _token = tokenElement.GetString();
            }
        }

        public async Task<List<MalzemeDto>> GetVwMaterialsAsync()
        {
            // /material/MalzemeleriGetir?ListeGrubu=VW&FirmaAdi=BASBUG&Depo=MRK
            var url = "/material/MalzemeleriGetir?ListeGrubu=VW&FirmaAdi=BASBUG&Depo=MRK";

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _client.SendAsync(req);
            response.EnsureSuccessStatusCode();

            var respJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // MalzemeleriGetirResponse -> malzemeListesi
            var result = JsonSerializer.Deserialize<MalzemeleriGetirResponse>(respJson, options);

            return result?.malzemeListesi ?? new List<MalzemeDto>();
        }

        public async Task<List<MalzemeDto>> GetOpelMaterialsAsync()
        {
            // /material/MalzemeleriGetir?ListeGrubu=OPEL&FirmaAdi=BASBUG&Depo=MRK
            var url = "/material/MalzemeleriGetir?ListeGrubu=OPEL&FirmaAdi=BASBUG&Depo=MRK";

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _client.SendAsync(req);
            response.EnsureSuccessStatusCode();

            var respJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var result = JsonSerializer.Deserialize<MalzemeleriGetirResponse>(respJson, options);
            return result?.malzemeListesi ?? new List<MalzemeDto>();
        }

        public async Task<List<MalzemeDto>> GetFordMaterialsAsync()
        {
            // /material/MalzemeleriGetir?ListeGrubu=FORD&FirmaAdi=BASBUG&Depo=MRK
            var url = "/material/MalzemeleriGetir?ListeGrubu=FORD&FirmaAdi=BASBUG&Depo=MRK";

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _client.SendAsync(req);
            response.EnsureSuccessStatusCode();

            var respJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var result = JsonSerializer.Deserialize<MalzemeleriGetirResponse>(respJson, options);
            return result?.malzemeListesi ?? new List<MalzemeDto>();
        }

        public async Task<List<MalzemeDto>> GetPSAMaterialsAsync()
        {
            // /material/MalzemeleriGetir?ListeGrubu=PSA&FirmaAdi=BASBUG&Depo=MRK
            var url = "/material/MalzemeleriGetir?ListeGrubu=PSA&FirmaAdi=BASBUG&Depo=MRK";

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _client.SendAsync(req);
            response.EnsureSuccessStatusCode();

            var respJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var result = JsonSerializer.Deserialize<MalzemeleriGetirResponse>(respJson, options);
            return result?.malzemeListesi ?? new List<MalzemeDto>();
        }

        public async Task<List<MalzemeDto>> GetFiatMaterialsAsync()
        {
            // /material/MalzemeleriGetir?ListeGrubu=PSA&FirmaAdi=BASBUG&Depo=MRK
            var url = "/material/MalzemeleriGetir?ListeGrubu=FIAT&FirmaAdi=BASBUG&Depo=MRK";

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _client.SendAsync(req);
            response.EnsureSuccessStatusCode();

            var respJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var result = JsonSerializer.Deserialize<MalzemeleriGetirResponse>(respJson, options);
            return result?.malzemeListesi ?? new List<MalzemeDto>();
        }

        public async Task<List<MalzemeDto>> GetRenaultMaterialsAsync()
        {
            // /material/MalzemeleriGetir?ListeGrubu=PSA&FirmaAdi=BASBUG&Depo=MRK
            var url = "/material/MalzemeleriGetir?ListeGrubu=RENAULT&FirmaAdi=BASBUG&Depo=MRK";

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _client.SendAsync(req);
            response.EnsureSuccessStatusCode();

            var respJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var result = JsonSerializer.Deserialize<MalzemeleriGetirResponse>(respJson, options);
            return result?.malzemeListesi ?? new List<MalzemeDto>();
        }

        public async Task<List<MalzemeDto>> GetAllMaterialsAsync(string make)
        {
            // /material/MalzemeleriGetir?ListeGrubu=PSA&FirmaAdi=BASBUG&Depo=MRK
            var url = $"/material/MalzemeleriGetir?ListeGrubu={make}&FirmaAdi=BASBUG&Depo=MRK";

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _client.SendAsync(req);
            response.EnsureSuccessStatusCode();

            var respJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var result = JsonSerializer.Deserialize<MalzemeleriGetirResponse>(respJson, options);
            return result?.malzemeListesi ?? new List<MalzemeDto>();
        }


        public async Task<List<MalzemeAraDto>> GetStockInfoByTopluMalzemeAra(string commaSeparatedNos)
        {
            // TopluMalzemeAra?FirmaAdi=BASBUG&MalzemeListesi=malzeme1,malzeme2,malzeme3
            // Virgül ile en az 2 malzemeNo olmalı, 6400 char limiti var
            var url = $"/material/TopluMalzemeAra?FirmaAdi=BASBUG&MalzemeListesi={commaSeparatedNos}";

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _client.SendAsync(req);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<MalzemeAraResponse>(json, options);

            return result?.malzemeListesi ?? new List<MalzemeAraDto>();
        }
    }
}