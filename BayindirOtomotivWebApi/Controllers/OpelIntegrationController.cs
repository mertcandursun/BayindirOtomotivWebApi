using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using BayindirOtomotivWebApi.Services;
using BayindirOtomotivWebApi.Models.Basbug;
using BayindirOtomotivWebApi.Helpers;
using BayindirOtomotivWebApi.Models;

namespace BayindirOtomotivWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpelIntegrationController : ControllerBase
    {
        private readonly BasbugService _basbugService;
        private readonly IWebHostEnvironment _env;
        private readonly GoogleImageService _googleImageService;
        private readonly IdeaSoftService _ideaSoftService;
        private readonly TecDocService _tecDocService;

        public OpelIntegrationController(
            BasbugService basbugService,
            IWebHostEnvironment env,
            GoogleImageService googleImageService,
            IdeaSoftService ideaSoftService,
            TecDocService tecDocService)
        {
            _basbugService = basbugService;
            _env = env;
            _googleImageService = googleImageService;
            _ideaSoftService = ideaSoftService;
            _tecDocService = tecDocService;
        }

        /* -------------------------------------------------- *
         * 1) OPEL malzeme listesi (ham)                      *
         * -------------------------------------------------- */
        [HttpGet("fetch-and-save-opel")]
        public async Task<IActionResult> FetchAndSaveOpelMaterials()
        {
            try
            {
                await _basbugService.AuthenticateAsync();

                var opelMaterials = await _basbugService.GetOpelMaterialsAsync();

                var filePath = Path.Combine(_env.ContentRootPath, "raw_Outputs/opel-materials.json");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                var json = JsonSerializer.Serialize(opelMaterials, new JsonSerializerOptions { WriteIndented = true });
                await System.IO.File.WriteAllTextAsync(filePath, json);

                return Ok(new { Count = opelMaterials.Count, FilePath = filePath });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /* -------------------------------------------------- *
         * 2) Stok + TecDoc görselleri                        *
         * -------------------------------------------------- */
        [HttpGet("fetch-stocks-and-images-opel")]
        public async Task<IActionResult> FetchStocksAndImagesOpel()
        {
            try
            {
                await _basbugService.AuthenticateAsync();

                var rawPath = Path.Combine(_env.ContentRootPath, "raw_Outputs/opel-materials.json");
                if (!System.IO.File.Exists(rawPath))
                    return BadRequest("raw_Outputs/opel-materials.json not found.");

                var rawJson = await System.IO.File.ReadAllTextAsync(rawPath);
                var rawItems = JsonSerializer.Deserialize<List<MalzemeDto>>(rawJson,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                if (rawItems.Count == 0) return BadRequest("opel-materials.json empty.");

                var noList = rawItems.Select(m => m.no).Distinct().ToList();
                const int CHUNK = 300;

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
                var outFile = Path.Combine(_env.ContentRootPath, "Outputs/opel-instock-all-updated.json");
                Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);

                await System.IO.File.WriteAllTextAsync(outFile,
                    JsonSerializer.Serialize(inStock, new JsonSerializerOptions { WriteIndented = true }));

                //if (failed.Count > 0)
                //{
                //    var failPath = Path.Combine(_env.ContentRootPath, "FailedPictures",
                //                    $"opel-fail-{DateTime.Now:yyyyMMdd-HHmm}.json");
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

        [HttpPut("update-opel-waiting")]
        public async Task<IActionResult> UpdateOpelProductsWaiting([FromQuery] string? code = null)
        {
            try
            {
                await EnsureIdeaSoftAuthAsync(code);

                /* 1) IdeaSoft listesi ------------------------------------------------ */
                const int opelCategoryId = 1;
                var ideaSoftItems = await _ideaSoftService.GetAllProductsForCategory(opelCategoryId, 100);
                if (ideaSoftItems.Count == 0)
                    return Ok("No Opel products found in IdeaSoft.");

                /* 2) SKU’ları Basbug’a gönder (200’lük parçalar) --------------------- */
                string Norm(string? s) => s?.Replace("/", " ").Replace("-", " ").Replace("  ", " ")
                                            .Trim().ToUpperInvariant() ?? "";

                var skuList = ideaSoftItems.Select(p => Norm(p.sku)).Distinct().ToList();
                const int CHUNK = 200;

                var waiting = new List<MalzemeAraDto>();
                await _basbugService.AuthenticateAsync();

                for (int i = 0; i < skuList.Count; i += CHUNK)
                {
                    var joined = string.Join(",", skuList.Skip(i).Take(CHUNK));
                    waiting.AddRange(await _basbugService.GetStockInfoByTopluMalzemeAra(joined));
                }

                /* 3) Waiting dosyasını kaydet --------------------------------------- */
                var waitingDir = Path.Combine(_env.ContentRootPath, "Waitingfor-Update");
                Directory.CreateDirectory(waitingDir);
                var waitingFile = Path.Combine(waitingDir, "opel-waiting.json");

                await System.IO.File.WriteAllTextAsync(
                    waitingFile,
                    JsonSerializer.Serialize(waiting, new JsonSerializerOptions { WriteIndented = true }));

                /* 4) UPDATE işlemi --------------------------------------------------- */
                var waitingDict = waiting.ToDictionary(w => Norm(w.no), w => w, StringComparer.Ordinal);
                var updated = 0;

                foreach (var prod in ideaSoftItems)
                {
                    if (!waitingDict.TryGetValue(Norm(prod.sku), out var src)) continue;

                    var dto = IdeasoftMapper.UpdateMapToIdeaSoftDto(src);
                    var resp = await _ideaSoftService.UpdateProductAsync(prod.id, dto);
                    if (resp.IsSuccess) updated++;

                    await Task.Delay(300);
                }

                return Ok($"Opel UPDATE done | Updated: {updated}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create-opel")]
        public async Task<IActionResult> CreateOpelProducts([FromQuery] string? code = null)
        {
            try
            {
                await EnsureIdeaSoftAuthAsync(code);

                var srcPath = Path.Combine(_env.ContentRootPath, "Outputs/opel-instock-all-updated.json");
                if (!System.IO.File.Exists(srcPath))
                    return BadRequest("opel-instock-all-updated.json not found.");

                var srcItems = JsonSerializer.Deserialize<List<MalzemeAraDto>>(
                                    await System.IO.File.ReadAllTextAsync(srcPath),
                                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                if (srcItems.Count == 0) return Ok("Source file empty.");

                /* IdeaSoft’ta var olan SKU’ları filtrele ----------------------------- */
                const int opelCategoryId = 1;
                var ideaSoftItems = await _ideaSoftService.GetAllProductsForCategory(opelCategoryId, 100);

                string Norm(string? s) => s?.Replace("/", " ").Replace("-", " ").Replace("  ", " ")
                                            .Trim().ToUpperInvariant() ?? "";

                var existingSkus = ideaSoftItems.Select(p => Norm(p.sku)).ToHashSet(StringComparer.Ordinal);

                var toCreate = srcItems.Where(s => !existingSkus.Contains(Norm(s.no))).ToList();
                var created = 0;

                foreach (var src in toCreate)
                {
                    var dto = IdeasoftMapper.MapToIdeaSoftDto(src);
                    var resp = await _ideaSoftService.CreateProductAsync(dto);
                    if (resp.IsSuccess) created++;

                    await Task.Delay(300);
                }

                return Ok($"Opel CREATE done | Created: {created}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /* -------------------------------------------------- *
         * 4) Tüm Opel ürünlerini listeleyip JSON kaydet      *
         * -------------------------------------------------- */
        [HttpGet("list-opel")]
        public async Task<IActionResult> ListOpelProducts([FromQuery] string? code = null)
        {
            try
            {
                await EnsureIdeaSoftAuthAsync(code);

                int opelCategoryId = 1;             // panelinizde farklıysa değiştirin
                var allProducts = await _ideaSoftService.GetAllProductsForCategory(opelCategoryId, 100);

                if (allProducts.Count == 0)
                    return Ok("No Opel products found for this category.");

                var filePath = Path.Combine(_env.ContentRootPath, "Product-List/opel-all.json");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                await System.IO.File.WriteAllTextAsync(
                    filePath,
                    JsonSerializer.Serialize(allProducts, new JsonSerializerOptions { WriteIndented = true }));

                return Ok($"Found {allProducts.Count} Opel products. Saved to {filePath}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        

        /* -------------------------------------------------- *
         * 6) Opel ürünlerini topluca sil                     *
         * -------------------------------------------------- */
        [HttpDelete("delete-all-opel-products")]
        public async Task<IActionResult> DeleteAllOpelProducts([FromQuery] string? code = null)
        {
            try
            {
                await EnsureIdeaSoftAuthAsync(code);

                var filePath = Path.Combine(_env.ContentRootPath, "Product-List/opel-all.json");
                if (!System.IO.File.Exists(filePath))
                    return BadRequest("opel-all.json not found.");

                var productList = JsonSerializer.Deserialize<List<IdeaSoftListingProductDto>>(
                                    await System.IO.File.ReadAllTextAsync(filePath),
                                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                int deleted = 0;
                foreach (var p in productList)
                {
                    if (p.id <= 0) continue;

                    var resp = await _ideaSoftService.DeleteProductAsync(p.id);
                    if (resp.IsSuccess) deleted++;

                    await Task.Delay(300);
                }

                return Ok($"Deleted {deleted} OPEL products.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task EnsureIdeaSoftAuthAsync(string? code)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                await _ideaSoftService.AcquireInitialTokensAsync(code);
            }
            else
            {
                await _ideaSoftService.GetAccessTokenAsync();
            }
        }
    }
}
