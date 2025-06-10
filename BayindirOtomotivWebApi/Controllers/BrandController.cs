using BayindirOtomotivWebApi.Helpers;
using BayindirOtomotivWebApi.Models;
using BayindirOtomotivWebApi.Models.Basbug;
using BayindirOtomotivWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;

namespace BayindirOtomotivWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandController : ControllerBase
    {
        private readonly BasbugService _basbugService;
        private readonly IWebHostEnvironment _env;
        private readonly IdeaSoftService _ideaSoftService;

        public BrandController(BasbugService basbugService, IWebHostEnvironment env, IdeaSoftService ideaSoftService)
        {
            _basbugService = basbugService;
            _env = env;
            _ideaSoftService = ideaSoftService;
        }

        // GET api/brand/fetch-light
        [HttpGet("fetch-light")]
        public async Task<IActionResult> FetchLightFiles()
        {
            string[] makes = {
                "BMW","FIAT","FORD","KORE",
                "MERCEDES","OPEL","PSA","RENAULT","VW"
            };

            var summary = new List<object>();

            await _basbugService.AuthenticateAsync();

            var lightDir = Path.Combine(_env.ContentRootPath, "light");
            Directory.CreateDirectory(lightDir);

            foreach (var make in makes.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var all = await _basbugService.GetAllMaterialsAsync(make);

                // “uk == DEPO” filter
                var filtered = all
                    .Where(m => string.Equals(m.uk, "DEPO", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // save to JSON
                var filePath = Path.Combine(lightDir, $"{make}-light.json");
                await System.IO.File.WriteAllTextAsync(
                    filePath,
                    JsonSerializer.Serialize(filtered, new JsonSerializerOptions { WriteIndented = true }));

                summary.Add(new { make, total = filtered.Count, filePath });
            }

            return Ok(summary);
        }

        [HttpGet("fetch-stocks-and-images-all-brandDepo")]
        public async Task<IActionResult> FetchStocksAndImagesAll()
        {
            try
            {
                await _basbugService.AuthenticateAsync();

                string[] makes = { "BMW","FIAT","FORD","KORE","MERCEDES",
                           "OPEL","PSA","RENAULT","VW" };

                var results = new List<object>();

                foreach (var make in makes.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    /* -------------------------------------------------- *
                     * 1) light klasöründen ham dosyayı oku               *
                     * -------------------------------------------------- */
                    var rawFile = Path.Combine(_env.ContentRootPath,
                                               $"light/{make.ToLower()}-light.json");
                    if (!System.IO.File.Exists(rawFile))
                    {
                        results.Add(new { make, error = "light file not found" });
                        continue;
                    }

                    var rawJson = await System.IO.File.ReadAllTextAsync(rawFile);
                    var rawItems = JsonSerializer.Deserialize<List<MalzemeDto>>(rawJson,
                                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                    if (rawItems.Count == 0)
                    {
                        results.Add(new { make, error = "light file empty" });
                        continue;
                    }

                    /* -------------------------------------------------- *
                     * 2) Stok çağrısı (chunk)                            *
                     * -------------------------------------------------- */
                    var noList = rawItems.Select(m => m.no).Distinct().ToList();
                    const int CHUNK = 200;

                    var allResults = new List<MalzemeAraDto>();
                    for (int i = 0; i < noList.Count; i += CHUNK)
                    {
                        var joined = string.Join(",", noList.Skip(i).Take(CHUNK));
                        var chunk = await _basbugService.GetStockInfoByTopluMalzemeAra(joined);
                        allResults.AddRange(chunk);
                    }

                    var inStock = allResults.Where(x =>
                                    (x.sMrk + x.sIzm + x.sAnk + x.sAdn + x.sErz) >= 1).ToList();

                    /* --- TecDoc görselleri (şimdilik kapalı) -----------------------
                    var failed = new List<MalzemeAraDto>();

                    foreach (var item in inStock)
                    {
                        if (string.IsNullOrWhiteSpace(item.oe)) { failed.Add(item); continue; }

                        var firstOe = item.oe.Split(' ')[0].Trim();
                        try
                        {
                            var td     = await _tecDocService.SearchArticleOEMAsync(firstOe);
                            var imgUrl = td?.articles?.FirstOrDefault()?.s3ImageLink;

                            if (!string.IsNullOrEmpty(imgUrl))
                            {
                                var b64 = await _tecDocService.DownloadImageAsBase64FromUrl(imgUrl);
                                if (!string.IsNullOrEmpty(b64)) item.imgBase64 = b64;
                                else failed.Add(item);
                            }
                            else failed.Add(item);
                        }
                        catch { failed.Add(item); }

                        await Task.Delay(200);   // rate‑limit koruması
                    }
                    ---------------------------------------------------------------- */

                    /* -------------------------------------------------- *
                     * 3) Çıktıyı kaydet                                 *
                     * -------------------------------------------------- */
                    var outFile = Path.Combine(_env.ContentRootPath,
                                               $"Outputs-light/{make.ToLower()}-brandDepo-all-updated.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);

                    await System.IO.File.WriteAllTextAsync(outFile,
                        JsonSerializer.Serialize(inStock, new JsonSerializerOptions { WriteIndented = true }));

                    /* failed dosyası – TecDoc’u açarsan aktif et
                    if (failed.Count > 0)
                    {
                        var failPath = Path.Combine(_env.ContentRootPath, "FailedPictures",
                                        $"{make.ToLower()}-fail-{DateTime.Now:yyyyMMdd-HHmm}.json");
                        Directory.CreateDirectory(Path.GetDirectoryName(failPath)!);

                        await System.IO.File.WriteAllTextAsync(failPath,
                            JsonSerializer.Serialize(failed, new JsonSerializerOptions { WriteIndented = true }));
                    }
                    */

                    results.Add(new
                    {
                        make,
                        total = inStock.Count,
                        base64Added = inStock.Count,
                        //base64Added = inStock.Count - failed.Count,
                        //failedCount = failed.Count,
                        outputFile = outFile
                    });
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update-lighting-waiting")]
        public async Task<IActionResult> UpdateLightingProductsWaiting([FromQuery] string? code = null)
        {
            try
            {
                await EnsureIdeaSoftAuthAsync(code);

                const int lightCategoryId = 1980;
                var ideaSoftItems = await _ideaSoftService.GetAllProductsForCategory(lightCategoryId, 100);
                if (ideaSoftItems.Count == 0)
                    return Ok("No products found in lighting category (1980).");

                string Norm(string? s) => s?
                    .Replace("/", " ").Replace("-", " ")
                    .Replace("  ", " ").Trim().ToUpperInvariant() ?? "";

                string[] makes = { "BMW","FIAT","FORD","KORE","MERCEDES",
                                   "OPEL","PSA","RENAULT","VW" };

                const int CHUNK = 200;
                int updated = 0;

                await _basbugService.AuthenticateAsync();

                foreach (var make in makes)
                {
                    /* --- markaya ait SKU listesi --- */
                    var skuList = ideaSoftItems
                        .Where(p => Norm(p.sku).StartsWith(make, StringComparison.Ordinal))
                        .Select(p => Norm(p.sku))
                        .Distinct()
                        .ToList();

                    int count = skuList.Count;          // Count property – parantez yok!
                    if (count == 0) continue;

                    // --- Basbug çağrıları ---
                    var waiting = new List<MalzemeAraDto>();
                    for (int i = 0; i < count; i += CHUNK)
                    {
                        var joined = string.Join(",", skuList.Skip(i).Take(CHUNK));
                        waiting.AddRange(await _basbugService.GetStockInfoByTopluMalzemeAra(joined));
                    }

                    /* --- Waiting dosyası --- */
                    var waitDir = Path.Combine(_env.ContentRootPath, "Waitingfor-Update-Lighting");
                    Directory.CreateDirectory(waitDir);
                    var waitFile = Path.Combine(waitDir, $"{make.ToLower()}-waiting.json");

                    await System.IO.File.WriteAllTextAsync(
                        waitFile,
                        JsonSerializer.Serialize(waiting, new JsonSerializerOptions { WriteIndented = true }));

                    var waitDict = waiting.ToDictionary(w => Norm(w.no), w => w, StringComparer.Ordinal);

                    /* --- UPDATE --- */
                    foreach (var prod in ideaSoftItems.Where(p => skuList.Contains(Norm(p.sku))))
                    {
                        if (!waitDict.TryGetValue(Norm(prod.sku), out var src)) continue;

                        var dto = IdeasoftMapper.UpdateMapToLightCategory(src);
                        var resp = await _ideaSoftService.UpdateProductAsync(prod.id, dto);
                        if (resp.IsSuccess) updated++;

                        await Task.Delay(350);
                    }
                }

                return Ok($"Lighting UPDATE done | Updated: {updated}");
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("create-lighting")]
        public async Task<IActionResult> CreateLightingProducts([FromQuery] string? code = null)
        {
            try
            {
                await EnsureIdeaSoftAuthAsync(code);

                const int lightCategoryId = 1980;
                var ideaSoftItems = await _ideaSoftService.GetAllProductsForCategory(lightCategoryId, 100);

                string Norm(string? s) => s?
                    .Replace("/", " ").Replace("-", " ")
                    .Replace("  ", " ").Trim().ToUpperInvariant() ?? "";

                var existingSkus = ideaSoftItems
                                   .Select(p => Norm(p.sku))
                                   .ToHashSet(StringComparer.Ordinal);

                string[] makes = { "BMW","FIAT","FORD","KORE","MERCEDES",
                                   "OPEL","PSA","RENAULT","VW" };

                int created = 0;

                foreach (var make in makes)
                {
                    var srcPath = Path.Combine(_env.ContentRootPath,
                                               $"Outputs-light/{make.ToLower()}-brandDepo-all-updated.json");
                    if (!System.IO.File.Exists(srcPath)) continue;

                    var srcItems = JsonSerializer.Deserialize<List<MalzemeAraDto>>(
                                       await System.IO.File.ReadAllTextAsync(srcPath),
                                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                    if (srcItems.Count == 0) continue;

                    var toCreate = srcItems.Where(s => !existingSkus.Contains(Norm(s.no))).ToList();

                    foreach (var src in toCreate)
                    {
                        var dto = IdeasoftMapper.CreateMapToLightCategory(src);
                        var resp = await _ideaSoftService.CreateProductAsync(dto);
                        if (resp.IsSuccess)
                        {
                            created++;
                            existingSkus.Add(Norm(src.no));
                        }
                        await Task.Delay(350);
                    }

                    /* Raporlama (opsiyonel) */
                    if (toCreate.Count > 0)
                    {
                        var missDir = Path.Combine(_env.ContentRootPath, "Missing-Update-Lighting");
                        Directory.CreateDirectory(missDir);
                        var missFile = Path.Combine(missDir,
                                      $"{make.ToLower()}-missing-{DateTime.Now:yyyyMMdd-HHmm}.json");

                        await System.IO.File.WriteAllTextAsync(
                            missFile,
                            JsonSerializer.Serialize(toCreate, new JsonSerializerOptions { WriteIndented = true }));
                    }
                }

                return Ok($"Lighting CREATE done | Created: {created}");
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

    private async Task EnsureIdeaSoftAuthAsync(string? code)
        {
            if (!string.IsNullOrWhiteSpace(code))
                await _ideaSoftService.AcquireInitialTokensAsync(code);
        }
    }
}
