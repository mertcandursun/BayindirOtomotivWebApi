// Services/IdeaSoftService.cs
using System.Net.Http.Headers;
using System.Text.Json;
using BayindirOtomotivWebApi.Infrastructure;
using BayindirOtomotivWebApi.Models;
using BayindirOtomotivWebApi.Models.Response;

namespace BayindirOtomotivWebApi.Services;

/// <summary>
/// IdeaSoft API ile tüm etkileşimlerin merkezi.
/// <para>• <see cref="AcquireInitialTokensAsync"/> - İlk “code” ile kalıcı Access / Refresh token’larını elde eder.</para>
/// <para>• <see cref="EnsureValidTokenAsync"/> - Her isteğin öncesinde çağrılarak token’ın süresi dolduysa
///     Refresh-Token akışı ile yeniler.</para>
/// Böylece kod tekrarına ve manuel token yönetimine gerek kalmaz.
/// </summary>
public sealed class IdeaSoftService
{
    /* ====================================================================== FIELDS */
    private readonly HttpClient _client;
    private readonly IdeaSoftTokenStore _store;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;

    /// <summary> Bellekte tutulan son başarılı token çifti. </summary>
    private TokenInfo? _token;

    /* ====================================================================== CTOR   */
    public IdeaSoftService(IHttpClientFactory factory,
                           IdeaSoftTokenStore store,
                           IConfiguration cfg)
    {
        _client = factory.CreateClient("IdeaSoftClient"); // Program.cs’te BaseAddress tanımlı
        _store = store;

        _clientId = cfg["IdeaSoft:ClientId"] ?? throw new ArgumentNullException("IdeaSoft:ClientId");
        _clientSecret = cfg["IdeaSoft:ClientSecret"] ?? throw new ArgumentNullException("IdeaSoft:ClientSecret");
        _redirectUri = cfg["IdeaSoft:RedirectUri"] ?? throw new ArgumentNullException("IdeaSoft:RedirectUri");
    }

    /* ====================================================================== TOKEN  */

    /// <summary>
    /// Tarayıcıdan dönen <c>code</c> ile ilk Access- &amp; Refresh-Token ikilisini alır
    /// ve kalıcı olarak <see cref="IdeaSoftTokenStore"/>’a kaydeder.
    /// Yalnızca <strong>ilk kurulumda</strong> çağrılır.
    /// </summary>
    public async Task AcquireInitialTokensAsync(string code)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret,
            ["redirect_uri"] = _redirectUri,
            ["code"] = code
        };

        _token = await RequestTokenAsync(form);
        await _store.SaveAsync(_token);
    }

    /// <summary>
    /// İsteklerden önce çağrılarak geçerli Access-Token döner.
    /// Süresi dolmak üzereyse “refresh_token” akışı ile sessizce yeniler.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Henüz <see cref="AcquireInitialTokensAsync"/> çağrılmamışsa.
    /// </exception>
    private async Task<string> EnsureValidTokenAsync()
    {
        _token ??= await _store.LoadAsync();
        if (_token is null)
            throw new InvalidOperationException("IdeaSoft token bulunamadı. İlk yetkilendirme yapılmamış.");

        // Artık yenileme yok; arka-plan job hallediyor
        return _token.AccessToken;
    }

    public async Task RefreshTokenIfNeededAsync()
    {
        var token = await _store.LoadAsync();
        if (token is null) return;

        // 2) 1 günden az kaldıysa yenile
        if (token.ExpiresAtUtc <= DateTime.UtcNow.AddDays(1))
        {
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["refresh_token"] = token.RefreshToken
            };

            var fresh = await RequestTokenAsync(form);
            await _store.SaveAsync(fresh);
        }
    }

    public async Task<string> GetAccessTokenAsync()
    {
        return await EnsureValidTokenAsync();
    }

    private async Task<TokenInfo> RequestTokenAsync(Dictionary<string, string> form)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "/oauth/v2/token")
        {
            Content = new FormUrlEncodedContent(form)
        };

        using var resp = await _client.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        var json = JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
        int expire = json.GetProperty("expires_in").GetInt32();

        return new TokenInfo
        {
            AccessToken = json.GetProperty("access_token").GetString()!,
            RefreshToken = json.GetProperty("refresh_token").GetString()!,
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(expire - 60)
        };
    }

    public async Task<IdeaSoftResponse> CreateProductAsync(IdeaSoftProductDto dto)
    {
        var token = await EnsureValidTokenAsync();

        using var req = new HttpRequestMessage(HttpMethod.Post, "/admin-api/products")
        { Content = JsonContent.Create(dto) };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var resp = await _client.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        return new IdeaSoftResponse { IsSuccess = resp.IsSuccessStatusCode, Message = body };
    }

    public async Task<IdeaSoftResponse> UpdateProductAsync(int id, IdeaSoftProductDto dto)
    {
        var token = await EnsureValidTokenAsync();

        using var req = new HttpRequestMessage(HttpMethod.Put, $"/admin-api/products/{id}")
        { Content = JsonContent.Create(dto) };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var resp = await _client.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        return new IdeaSoftResponse { IsSuccess = resp.IsSuccessStatusCode, Message = body };
    }

    public async Task<IdeaSoftResponse> DeleteProductAsync(int id)
    {
        var token = await EnsureValidTokenAsync();

        using var req = new HttpRequestMessage(HttpMethod.Delete, $"/admin-api/products/{id}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var resp = await _client.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        return new IdeaSoftResponse { IsSuccess = resp.IsSuccessStatusCode, Message = body };
    }

    /* ====================================================================== LISTING */

    public Task<List<IdeaSoftListingProductDto>> GetAllProductsForCategory(int categoryId, int limit = 100) =>
        PagedFetchAsync<IdeaSoftListingProductDto>($"/admin-api/products?category={categoryId}", limit);

    // Services/IdeaSoftService.cs  (yeni sürüm)
    /// <summary>
    /// IdeaSoft’tan tüm kategorileri (alt + üst) çeker.
    /// – Tek kriter: <paramref name="limit"/> (1-500 arası)
    /// – sayfa bitene kadar page++ yapar.  (sinceId’i değiştirme!)
    /// </summary>
    public async Task<List<IdeaSoftListingCategoryDto>> GetAllCategoriesAsync(
        int limit = 100,       // IdeaSoft max 500
        int sinceId = 1)       // ‘1’ = kök; değiştirmeye gerek yok
    {
        var result = new List<IdeaSoftListingCategoryDto>();
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var token = await EnsureValidTokenAsync();

        for (int page = 1; ; page++)
        {
            string url = $"/admin-api/categories?sinceId={sinceId}&limit={limit}&page={page}";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var resp = await _client.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"IdeaSoft categories call failed: {(int)resp.StatusCode} {resp.ReasonPhrase}");

            var chunk = JsonSerializer.Deserialize<List<IdeaSoftListingCategoryDto>>(
                            await resp.Content.ReadAsStringAsync(), opts) ?? new();

            if (chunk.Count == 0)           // boş liste → son sayfa
                break;

            result.AddRange(chunk);

            if (chunk.Count < limit)        // normal bitiş (ör. son sayfa 37 kayıt)
                break;
        }

        return result;
    }




    /* ====================================================================== HELPERS */

    private async Task<List<T>> PagedFetchAsync<T>(string baseUrl, int limit) where T : class
    {
        var list = new List<T>();
        int page = 1;
        var opt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var token = await EnsureValidTokenAsync();

        while (true)
        {
            var url = $"{baseUrl}&page={page}&limit={limit}";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var resp = await _client.SendAsync(req);
            if (!resp.IsSuccessStatusCode) break;

            var chunk = JsonSerializer.Deserialize<List<T>>(await resp.Content.ReadAsStringAsync(), opt);
            if (chunk is null || chunk.Count == 0) break;

            list.AddRange(chunk);
            if (chunk.Count < limit) break;   // son sayfa
            page++;
        }

        return list;
    }
}
