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
    public class FiatIntegrationController : ControllerBase
    {
        private readonly BasbugService _basbugService;
        private readonly IWebHostEnvironment _env;
        private readonly IdeaSoftService _ideaSoftService;
        private readonly TecDocService _tecDocService;

        public FiatIntegrationController(
            BasbugService basbugService,
            IWebHostEnvironment env,
            IdeaSoftService ideaSoftService,
            TecDocService tecDocService
        )
        {
            _basbugService = basbugService;
            _env = env;
            //_googleImageService = googleImageService;
            _ideaSoftService = ideaSoftService;
            _tecDocService = tecDocService;
        }


        [HttpGet("fetch-and-save-fiat")]
        public async Task<IActionResult> FetchAndSaveFiatMaterials()
        {
            try
            {
                await _basbugService.AuthenticateAsync();

                var fiatMaterials = await _basbugService.GetFiatMaterialsAsync();

                var filePath = Path.Combine(_env.ContentRootPath, "raw_Outputs/fiat-materials.json");
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(fiatMaterials, options);
                await System.IO.File.WriteAllTextAsync(filePath, json);

                return Ok(new { Count = fiatMaterials.Count, FilePath = filePath });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("fetch-stocks-and-images-fiat")]
        public async Task<IActionResult> FetchStocksAndImagesFiat()
        {
            try
            {
                await _basbugService.AuthenticateAsync();

                var rawPath = Path.Combine(_env.ContentRootPath, "raw_Outputs/fiat-materials.json");
                if (!System.IO.File.Exists(rawPath))
                    return BadRequest("raw_Outputs/fiat-materials.json not found.");

                var rawJson = await System.IO.File.ReadAllTextAsync(rawPath);
                var rawItems = JsonSerializer.Deserialize<List<MalzemeDto>>(rawJson,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                if (rawItems.Count == 0)
                    return BadRequest("fiat-materials.json empty.");

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
                var outFile = Path.Combine(_env.ContentRootPath, "Outputs/fiat-instock-all-updated.json");
                Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);

                await System.IO.File.WriteAllTextAsync(outFile,
                    JsonSerializer.Serialize(inStock, new JsonSerializerOptions { WriteIndented = true }));

                //if (failed.Count > 0)
                //{
                //    var failPath = Path.Combine(_env.ContentRootPath, "FailedPictures",
                //                    $"fiat-fail-{DateTime.Now:yyyyMMdd-HHmm}.json");
                //    Directory.CreateDirectory(Path.GetDirectoryName(failPath)!);

                //    await System.IO.File.WriteAllTextAsync(failPath,
                //        JsonSerializer.Serialize(failed, new JsonSerializerOptions { WriteIndented = true }));
                //}

                return Ok(new
                {
                    total = inStock.Count,
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


        [HttpGet("list-fiat")]
        public async Task<IActionResult> ListFiatProducts([FromQuery] string? code = null)
        {
            try
            {
                await EnsureIdeaSoftAuthAsync(code);

                // 1) Get parts parts all FIAT materials in ideaSoft (limit=100)
                int fiatCategoryId = 6;
                var allProducts = await _ideaSoftService.GetAllProductsForCategory(fiatCategoryId, 100);

                // 2) Product count in cache
                int totalCount = allProducts.Count;
                if (totalCount == 0)
                    return Ok("No Fiat products found for this category.");

                // write to file
                var filePath = Path.Combine(_env.ContentRootPath, "Product-List/fiat-all.json");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                var options = new JsonSerializerOptions { WriteIndented = true };
                var jsonString = JsonSerializer.Serialize(allProducts, options);

                await System.IO.File.WriteAllTextAsync(filePath, jsonString);

                return Ok($"Found {totalCount} Fiat products. Saved to {filePath}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update-fiat-waiting")]
        public async Task<IActionResult> UpdateFiatProductsWaiting([FromQuery] string? code = null)
        {
            try
            {
                await EnsureIdeaSoftAuthAsync(code);

                /* 1) IdeaSoft listesi ---------------------------------------------- */
                const int fiatCategoryId = 6;
                var ideaSoftItems = await _ideaSoftService.GetAllProductsForCategory(fiatCategoryId, 100);
                if (ideaSoftItems.Count == 0)
                    return Ok("No Fiat products found in IdeaSoft.");

                /* 2) SKU’ları 200’lük parçalarla Basbug’a gönder -------------------- */
                string Norm(string? s) => s?.Replace("/", " ").Replace("-", " ")
                                            .Replace("  ", " ").Trim().ToUpperInvariant() ?? "";

                var skuList = ideaSoftItems.Select(p => Norm(p.sku)).Distinct().ToList();
                const int CHUNK = 200;

                var waiting = new List<MalzemeAraDto>();
                await _basbugService.AuthenticateAsync();

                for (int i = 0; i < skuList.Count; i += CHUNK)
                {
                    var joined = string.Join(",", skuList.Skip(i).Take(CHUNK));
                    waiting.AddRange(await _basbugService.GetStockInfoByTopluMalzemeAra(joined));
                }

                /* 3) Waiting dosyasını kaydet -------------------------------------- */
                var waitingDir = Path.Combine(_env.ContentRootPath, "Waitingfor-Update");
                Directory.CreateDirectory(waitingDir);
                var waitingFile = Path.Combine(waitingDir, "fiat-waiting.json");

                await System.IO.File.WriteAllTextAsync(
                    waitingFile,
                    JsonSerializer.Serialize(waiting, new JsonSerializerOptions { WriteIndented = true }));

                /* 4) UPDATE --------------------------------------------------------- */
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

                return Ok($"FIAT UPDATE done | Updated: {updated}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create-fiat")]
        public async Task<IActionResult> CreateFiatProducts([FromQuery] string? code = null)
        {
            try
            {
                await EnsureIdeaSoftAuthAsync(code);

                var srcPath = Path.Combine(_env.ContentRootPath, "Outputs/fiat-instock-all-updated.json");
                if (!System.IO.File.Exists(srcPath))
                    return BadRequest("fiat-instock-all-updated.json not found.");

                var srcItems = JsonSerializer.Deserialize<List<MalzemeAraDto>>(
                                    await System.IO.File.ReadAllTextAsync(srcPath),
                                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                if (srcItems.Count == 0) return Ok("Source file empty.");

                /* IdeaSoft’ta var olan SKU’ları filtrele ---------------------------- */
                const int fiatCategoryId = 6;
                var ideaSoftItems = await _ideaSoftService.GetAllProductsForCategory(fiatCategoryId, 100);

                string Norm(string? s) => s?.Replace("/", " ").Replace("-", " ")
                                            .Replace("  ", " ").Trim().ToUpperInvariant() ?? "";

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

                return Ok($"FIAT CREATE done | Created: {created}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[HttpDelete("delete-all-fiat-products")]
        //public async Task<IActionResult> DeleteAllFiatProducts([FromQuery] string code)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(code))
        //            return BadRequest("code parameter is required.");

        //        // 1) Access token alın (varsa yeniden token alabilirsiniz)
        //        await _ideaSoftService.GetAccessTokenAsync(code);

        //        // 2) "fiat-all.json" dosyasını oku (Product-List klasörü içinde)
        //        var filePath = Path.Combine(_env.ContentRootPath, "Product-List/fiat-all.json");
        //        if (!System.IO.File.Exists(filePath))
        //            return BadRequest("fiat-all.json not found.");

        //        var jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
        //        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        //        var productList = JsonSerializer.Deserialize<List<IdeaSoftListingProductDto>>(jsonContent, options);

        //        if (productList == null || productList.Count == 0)
        //            return BadRequest("fiat-all.json is empty.");

        //        int deletedCount = 0;
        //        // Tüm ürünler için delete çağrısı yapalım
        //        foreach (var product in productList)
        //        {
        //            // Sadece geçerli ID'ye sahip ürünler için
        //            if (product.id <= 0)
        //                continue;

        //            var deleteResponse = await _ideaSoftService.DeleteProductAsync(product.id);
        //            if (deleteResponse.IsSuccess)
        //            {
        //                deletedCount++;
        //            }
        //            else
        //            {
        //                // İsteğe bağlı: loglama yapabilirsiniz
        //                Console.WriteLine($"Failed to delete product {product.id}: {deleteResponse.Message}");
        //            }
        //            // Rate-limit nedeniyle her istek arasında kısa gecikme ekleyelim (örneğin 300 ms)
        //            await Task.Delay(300);
        //        }

        //        return Ok($"Deleted {deletedCount} Fiat products.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        private async Task EnsureIdeaSoftAuthAsync(string? code)
        {
            if (!string.IsNullOrWhiteSpace(code))
                await _ideaSoftService.AcquireInitialTokensAsync(code);
        }
    }
}
