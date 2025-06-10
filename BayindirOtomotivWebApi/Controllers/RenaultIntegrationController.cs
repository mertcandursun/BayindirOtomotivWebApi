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
    public class RenaultIntegrationController : ControllerBase
    {
        private readonly BasbugService _basbugService;
        private readonly IWebHostEnvironment _env;
        //private readonly GoogleImageService _googleImageService;
        private readonly IdeaSoftService _ideaSoftService;
        private readonly TecDocService _tecDocService;

        public RenaultIntegrationController(BasbugService basbugService, IWebHostEnvironment env, IdeaSoftService ideaSoftService, TecDocService tecDocService)
        {
            _basbugService = basbugService;
            _env = env;
            _ideaSoftService = ideaSoftService;
            _tecDocService = tecDocService;
        }

        /* -------------------------------------------------- *
         * 1) Renault malzeme listesi (ham)                   *
         * -------------------------------------------------- */
        [HttpGet("fetch-and-save-renault")]
        public async Task<IActionResult> FetchAndSaveRenaultMaterials()
        {
            try
            {
                await _basbugService.AuthenticateAsync();

                var renaultMaterials = await _basbugService.GetRenaultMaterialsAsync();

                var filePath = Path.Combine(_env.ContentRootPath, "raw_Outputs/renault-materials.json");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                var json = JsonSerializer.Serialize(renaultMaterials, new JsonSerializerOptions { WriteIndented = true });
                await System.IO.File.WriteAllTextAsync(filePath, json);

                return Ok(new { Count = renaultMaterials.Count, FilePath = filePath });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /* -------------------------------------------------- *
         * 2) Stok + TecDoc görselleri                        *
         * -------------------------------------------------- */
        [HttpGet("fetch-stocks-and-images-renault")]
        public async Task<IActionResult> FetchStocksAndImagesRenault()
        {
            try
            {
                await _basbugService.AuthenticateAsync();

                var rawPath = Path.Combine(_env.ContentRootPath, "raw_Outputs/renault-materials.json");
                if (!System.IO.File.Exists(rawPath))
                    return BadRequest("raw_Outputs/renault-materials.json not found.");

                var rawJson = await System.IO.File.ReadAllTextAsync(rawPath);
                var rawItems = JsonSerializer.Deserialize<List<MalzemeDto>>(rawJson,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                if (rawItems.Count == 0) return BadRequest("renault-materials.json empty.");

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

                /* --- TecDoc görselleri ------------------------------------------------ */
                //var failed = new List<MalzemeAraDto>();

                //foreach (var item in inStock)
                //{
                //    if (string.IsNullOrWhiteSpace(item.oe)) { failed.Add(item); continue; }

                //    var firstOe = item.oe.Split(' ')[0].Trim();
                //    try
                //    {
                //        var td = await _tecDocService.SearchArticleOEMAsync(firstOe);
                //        var imgUrl = td?.articles?.FirstOrDefault()?.s3ImageLink;

                //        if (!string.IsNullOrEmpty(imgUrl))
                //        {
                //            var b64 = await _tecDocService.DownloadImageAsBase64FromUrl(imgUrl);
                //            if (!string.IsNullOrEmpty(b64)) item.imgBase64 = b64;
                //            else failed.Add(item);
                //        }
                //        else failed.Add(item);
                //    }
                //    catch { failed.Add(item); }

                //    await Task.Delay(200);   // rate-limit koruması
                //}

                /* --- Çıktıları kaydet -------------------------------------------------- */
                var outFile = Path.Combine(_env.ContentRootPath, "Outputs/renault-instock-all-updated.json");
                Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);

                await System.IO.File.WriteAllTextAsync(outFile,
                    JsonSerializer.Serialize(inStock, new JsonSerializerOptions { WriteIndented = true }));

                //if (failed.Count > 0)
                //{
                //    var failPath = Path.Combine(_env.ContentRootPath, "FailedPictures",
                //                    $"renault-fail-{DateTime.Now:yyyyMMdd-HHmm}.json");
                //    Directory.CreateDirectory(Path.GetDirectoryName(failPath)!);

                //    await System.IO.File.WriteAllTextAsync(failPath,
                //        JsonSerializer.Serialize(failed, new JsonSerializerOptions { WriteIndented = true }));
                //}

                return Ok(new
                {
                    total = inStock.Count,
                    base64Added = inStock.Count,
                    //base64Added = inStock.Count - failed.Count,
                    //failedCount = failed.Count,
                    outputFile = outFile
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update-renault-waiting")]
        public async Task<IActionResult> UpdateRenaultProductsAsync([FromQuery] string? code = null)
        {
            try
            {
                await EnsureIdeaSoftAuthAsync(code);

                /* --- IdeaSoft listesi ------------------------------------------------ */
                const int renaultCategoryId = 5;
                var ideaSoftItems = await _ideaSoftService.GetAllProductsForCategory(renaultCategoryId, 100);
                if (ideaSoftItems.Count == 0) return Ok("No Renault products found in IdeaSoft.");

                /* --- SKU listesini 200’lük chunk’larla Basbug’a gönder ---------------- */
                string Norm(string? s) => s?
                    .Replace("/", " ").Replace("-", " ")
                    .Replace("  ", " ").Trim().ToUpperInvariant() ?? "";

                var skuList = ideaSoftItems.Select(p => Norm(p.sku)).Distinct().ToList();
                const int CHUNK = 200;

                var waiting = new List<MalzemeAraDto>();
                await _basbugService.AuthenticateAsync();

                for (int i = 0; i < skuList.Count; i += CHUNK)
                {
                    var joined = string.Join(",", skuList.Skip(i).Take(CHUNK));
                    var chunk = await _basbugService.GetStockInfoByTopluMalzemeAra(joined);
                    waiting.AddRange(chunk);
                }

                /* --- Waiting dosyasını kaydet ---------------------------------------- */
                var waitingDir = Path.Combine(_env.ContentRootPath, "Waitingfor-Update");
                Directory.CreateDirectory(waitingDir);
                var waitingFile = Path.Combine(waitingDir, "renault-waiting.json");

                await System.IO.File.WriteAllTextAsync(
                    waitingFile,
                    JsonSerializer.Serialize(waiting, new JsonSerializerOptions { WriteIndented = true }));

                /* --- Update işlemi ---------------------------------------------------- */
                var waitingDict = waiting.ToDictionary(w => Norm(w.no), w => w, StringComparer.Ordinal);
                var updated = 0;

                foreach (var prod in ideaSoftItems)
                {
                    if (!waitingDict.TryGetValue(Norm(prod.sku), out var src)) continue;

                    var dto = IdeasoftMapper.UpdateMapToIdeaSoftDto(src);
                    var resp = await _ideaSoftService.UpdateProductAsync(prod.id, dto);
                    if (resp.IsSuccess) updated++;

                    await Task.Delay(350);   // IdeaSoft rate‑limit
                }

                return Ok($"Renault UPDATE done | Updated: {updated}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create-renault")]
        public async Task<IActionResult> CreateRenaultProductsAsync([FromQuery] string? code = null)
        {
            try
            {
                await EnsureIdeaSoftAuthAsync(code);

                var srcPath = Path.Combine(_env.ContentRootPath, "Outputs/renault-instock-all-updated.json");
                if (!System.IO.File.Exists(srcPath))
                    return BadRequest("renault-instock-all-updated.json not found.");

                var srcItems = JsonSerializer.Deserialize<List<MalzemeAraDto>>(
                                    await System.IO.File.ReadAllTextAsync(srcPath),
                                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                if (srcItems.Count == 0) return Ok("Source file empty.");

                /* --- IdeaSoft’ta var olan SKU’ları alıp filtrele ---------------------- */
                const int renaultCategoryId = 5;
                var ideaSoftItems = await _ideaSoftService.GetAllProductsForCategory(renaultCategoryId, 100);
                string Norm(string? s) => s?
                    .Replace("/", " ").Replace("-", " ")
                    .Replace("  ", " ").Trim().ToUpperInvariant() ?? "";

                var existingSkus = ideaSoftItems.Select(p => Norm(p.sku)).ToHashSet(StringComparer.Ordinal);

                var toCreate = srcItems.Where(s => !existingSkus.Contains(Norm(s.no))).ToList();
                var created = 0;

                foreach (var src in toCreate)
                {
                    var dto = IdeasoftMapper.MapToIdeaSoftDto(src);
                    var resp = await _ideaSoftService.CreateProductAsync(dto);
                    if (resp.IsSuccess) created++;

                    await Task.Delay(350);
                }

                return Ok($"Renault CREATE done | Created: {created}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task EnsureIdeaSoftAuthAsync(string? code)
        {
            if (!string.IsNullOrWhiteSpace(code))
                await _ideaSoftService.AcquireInitialTokensAsync(code);
        }
    }
}
