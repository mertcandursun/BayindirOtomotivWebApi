using Microsoft.AspNetCore.Mvc;
using BayindirOtomotivWebApi.Services;
using System.Text.Json;
using BayindirOtomotivWebApi.Models.Basbug;
using BayindirOtomotivWebApi.Models;
using BayindirOtomotivWebApi.Helpers;

namespace BayindirOtomotivWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VWIntegrationController : ControllerBase
    {
        private readonly BasbugService _basbugService;
        private readonly IWebHostEnvironment _env; // Dosya yolu için kullanabiliriz
        private readonly GoogleImageService _googleImageService;
        private readonly IdeaSoftService _ideaSoftService;
        private readonly TecDocService _tecDocService;

        public VWIntegrationController(BasbugService basbugService, IWebHostEnvironment env, GoogleImageService googleImageService, IdeaSoftService ideaSoftService, TecDocService tecDocService)
        {
            _basbugService = basbugService;
            _env = env;
            _googleImageService = googleImageService;
            _ideaSoftService = ideaSoftService;
            _tecDocService = tecDocService;
        }

        [HttpGet("fetch-and-save-vw")]
        public async Task<IActionResult> FetchAndSaveVwMaterials()
        {
            try
            {
                // 1) Basbug'a login (token)
                await _basbugService.AuthenticateAsync();

                // 2) Sadece "VW" liste grubunu çek
                var vwMaterials = await _basbugService.GetVwMaterialsAsync();

                // 3) JSON kaydetme vb.
                var filePath = Path.Combine(_env.ContentRootPath, "raw_Outputs/vw-materials.json");
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(vwMaterials, options);
                await System.IO.File.WriteAllTextAsync(filePath, json);

                return Ok(new { Count = vwMaterials.Count, FilePath = filePath });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("fetch-stocks-vw")]
        public async Task<IActionResult> FetchStocksAndImagesVw()
        {
            try
            {
                await _basbugService.AuthenticateAsync();

                var rawPath = Path.Combine(_env.ContentRootPath, "raw_Outputs/vw-materials.json");
                if (!System.IO.File.Exists(rawPath))
                    return BadRequest("raw_Outputs/vw-materials.json not found.");

                var rawJson = await System.IO.File.ReadAllTextAsync(rawPath);
                var rawItems = JsonSerializer.Deserialize<List<MalzemeDto>>(rawJson,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                if (rawItems.Count == 0)
                    return BadRequest("vw-materials.json empty.");

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

                /* --- TecDoc görselleri (isteğe bağlı) -------------------------------
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

                    await Task.Delay(200);   // rate-limit koruması
                }
                ---------------------------------------------------------------------*/

                /* --- Çıktıları kaydet --------------------------------------------- */
                var outFile = Path.Combine(_env.ContentRootPath, "Outputs/vw-instock-all-updated.json");
                Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);

                await System.IO.File.WriteAllTextAsync(outFile,
                    JsonSerializer.Serialize(inStock, new JsonSerializerOptions { WriteIndented = true }));

                // Eğer TecDoc bloğunu aktifleştirirsen failed listesini de kaydedebilirsin
                /*
                if (failed.Count > 0)
                {
                    var failPath = Path.Combine(_env.ContentRootPath, "FailedPictures",
                                    $"vw-fail-{DateTime.Now:yyyyMMdd-HHmm}.json");
                    Directory.CreateDirectory(Path.GetDirectoryName(failPath)!);

                    await System.IO.File.WriteAllTextAsync(failPath,
                        JsonSerializer.Serialize(failed, new JsonSerializerOptions { WriteIndented = true }));
                }
                */

                return Ok(new
                {
                    total = inStock.Count,
                    base64Added = inStock.Count,
                    outputFile = outFile
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update-vw-waiting")]
        public async Task<IActionResult> UpdateVwProductsWaiting([FromQuery] string? code = null)
        {
            try
            {
                await EnsureIdeaSoftAuthAsync(code);

                /* 1) IdeaSoft’taki mevcut VW ürünleri ----------------------------- */
                const int vwCategoryId = 7;
                var ideaSoftItems = await _ideaSoftService.GetAllProductsForCategory(vwCategoryId, 100);
                if (ideaSoftItems.Count == 0)
                    return Ok("No VW products found in IdeaSoft.");

                /* 2) SKU listesini 200’lük chunk’larla Basbug’a gönder ------------ */
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

                /* 3) Waiting dosyasını kaydet ------------------------------------ */
                var waitDir = Path.Combine(_env.ContentRootPath, "Waitingfor-Update");
                Directory.CreateDirectory(waitDir);
                var waitFile = Path.Combine(waitDir, "vw-waiting.json");

                await System.IO.File.WriteAllTextAsync(
                    waitFile,
                    JsonSerializer.Serialize(waiting, new JsonSerializerOptions { WriteIndented = true }));

                /* 4) UPDATE ------------------------------------------------------- */
                var waitDict = waiting.ToDictionary(w => Norm(w.no), w => w, StringComparer.Ordinal);
                var updated = 0;

                foreach (var prod in ideaSoftItems)
                {
                    if (!waitDict.TryGetValue(Norm(prod.sku), out var src)) continue;

                    var dto = IdeasoftMapper.UpdateMapToIdeaSoftDto(src);
                    var resp = await _ideaSoftService.UpdateProductAsync(prod.id, dto);
                    if (resp.IsSuccess) updated++;

                    await Task.Delay(300);
                }

                return Ok($"VW UPDATE done | Updated: {updated}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create-vw")]
        public async Task<IActionResult> CreateVwProducts([FromQuery] string? code = null)
        {
            try
            {
                await EnsureIdeaSoftAuthAsync(code);

                var srcPath = Path.Combine(_env.ContentRootPath, "Outputs/vw-instock-all-updated.json");
                if (!System.IO.File.Exists(srcPath))
                    return BadRequest("vw-instock-all-updated.json not found.");

                var srcItems = JsonSerializer.Deserialize<List<MalzemeAraDto>>(
                                   await System.IO.File.ReadAllTextAsync(srcPath),
                                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                if (srcItems.Count == 0) return Ok("Source file empty.");

                /* IdeaSoft’ta var olan SKU’ları al ------------------------------- */
                const int vwCategoryId = 7;
                var ideaSoftItems = await _ideaSoftService.GetAllProductsForCategory(vwCategoryId, 100);

                string Norm(string? s) => s?.Replace("/", " ").Replace("-", " ")
                                             .Replace("  ", " ").Trim().ToUpperInvariant() ?? "";

                var existing = ideaSoftItems.Select(p => Norm(p.sku))
                                            .ToHashSet(StringComparer.Ordinal);

                var toCreate = srcItems.Where(s => !existing.Contains(Norm(s.no))).ToList();
                var created = 0;

                foreach (var src in toCreate)
                {
                    var dto = IdeasoftMapper.MapToIdeaSoftDto(src);
                    var resp = await _ideaSoftService.CreateProductAsync(dto);
                    if (resp.IsSuccess) created++;

                    await Task.Delay(300);
                }

                return Ok($"VW CREATE done | Created: {created}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        //[HttpGet("update-tecdoc-images-vw")]
        //public async Task<IActionResult> UpdateImagesFromTecDoc()
        //{
        //    int count = 0;
        //    try
        //    {
        //        // Step 1: Read the "vw-instock-all.json" file
        //        var filePath = Path.Combine(_env.ContentRootPath, "Outputs/vw-instock-all.json");
        //        if (!System.IO.File.Exists(filePath))
        //            return BadRequest("vw-instock-all.json not found.");

        //        var jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
        //        var options = new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true,
        //            WriteIndented = true
        //        };

        //        // Deserialize the JSON into the product list.
        //        var productList = JsonSerializer.Deserialize<List<MalzemeAraDto>>(jsonContent, options);
        //        if (productList == null || productList.Count == 0)
        //            return BadRequest("vw-instock-all.json is empty.");

        //        // Create a list to track failed image updates.
        //        var failedProducts = new List<MalzemeAraDto>();

        //        // Step 2: Loop through all products (no limit imposed)
        //        foreach (var product in productList)
        //        {
        //            if (string.IsNullOrEmpty(product.oe))
        //                continue;

        //            // If the "oe" value is like "6806612 1808604 1808601", take the code before the first space.
        //            var firstOe = product.oe.Split(' ')[0].Trim();

        //            try
        //            {
        //                // Call the TecDoc service to search for the article by OEM code.
        //                if (count != productList.Count)
        //                {
        //                    var tecDocResponse = await _tecDocService.SearchArticleOEMAsync(firstOe);
        //                    if (tecDocResponse?.articles != null && tecDocResponse.articles.Count > 0)
        //                    {
        //                        // Get the s3ImageLink from the first article in the response.
        //                        var imageUrl = tecDocResponse.articles[0].s3ImageLink;
        //                        // Download the image from the URL and convert it to Base64.
        //                        var base64Image = await _tecDocService.DownloadImageAsBase64FromUrl(imageUrl);
        //                        if (!string.IsNullOrEmpty(base64Image))
        //                        {
        //                            product.imgBase64 = base64Image;
        //                        }
        //                        else
        //                        {
        //                            // If conversion fails (empty result), add to failed list.
        //                            failedProducts.Add(product);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        // If no articles were returned, add product to failed list.
        //                        failedProducts.Add(product);
        //                    }
        //                    count++;
        //                }
        //                else
        //                {
        //                    break;
        //                }
        //            }
        //            catch (HttpRequestException ex)
        //            {
        //                // Log the exception and add the product to the failed list.
        //                Console.WriteLine($"Request for product {product.oe} failed: {ex.Message}");
        //                failedProducts.Add(product);
        //            }

        //            // Delay 1 second between requests to reduce rate limit issues.
        //            await Task.Delay(250);
        //        }

        //        // Step 3: Write the updated product list to a JSON file
        //        var newFilePath = Path.Combine(_env.ContentRootPath, "Outputs/vw-instock-all-updated.json");
        //        Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
        //        var updatedJson = JsonSerializer.Serialize(productList, options);
        //        await System.IO.File.WriteAllTextAsync(newFilePath, updatedJson);

        //        // Step 4: Write failed products to a separate JSON file if there are any.
        //        if (failedProducts.Count > 0)
        //        {
        //            // Create a folder for failed pictures if it does not exist.
        //            var failedFolder = Path.Combine(_env.ContentRootPath, "FailedPictures");
        //            Directory.CreateDirectory(failedFolder);

        //            // Use today's date as part of the filename.
        //            var today = DateTime.Now.ToString("dd-MM-yyyy");
        //            var failedFilePath = Path.Combine(failedFolder, $"vw-fail-pictures-{today}.json");
        //            var failedJson = JsonSerializer.Serialize(failedProducts, options);
        //            await System.IO.File.WriteAllTextAsync(failedFilePath, failedJson);
        //        }

        //        return Ok($"Updated images for {productList.Count} products. Saved to: {newFilePath}");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[HttpGet("vw-import")]
        //public async Task<IActionResult> ImportToIdeaSoft([FromQuery] string code)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(code))
        //            return BadRequest("code parametresi gerekli.");

        //        // 1) Retrieve an access token from IdeaSoft using the provided code.
        //        await _ideaSoftService.GetAccessTokenAsync(code);

        //        // 2) Read the Basbug JSON file that contains VW product data.
        //        var filePath = Path.Combine(_env.ContentRootPath, "Outputs/vw-instock-all-updated.json");
        //        if (!System.IO.File.Exists(filePath))
        //            return BadRequest("vw-instock-all-updated.json not found.");

        //        var json = await System.IO.File.ReadAllTextAsync(filePath);
        //        var items = JsonSerializer.Deserialize<List<MalzemeAraDto>>(json);

        //        if (items == null || items.Count == 0)
        //            return BadRequest("vw-instock-all-updated.json is empty.");

        //        // 3) Loop through each product and map it to an IdeaSoft product DTO,
        //        // then create that product in IdeaSoft.
        //        int created = 0;
        //        foreach (var item in items)
        //        {
        //            var productDto = IdeasoftMapper.MapToIdeaSoftDto(item); // Mapping handles price, tax, images, etc.
        //            var resp = await _ideaSoftService.CreateProductAsync(productDto);
        //            if (resp.IsSuccess)
        //                created++;
        //        }

        //        return Ok($"Imported {created} VW products to IdeaSoft.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}


        //[HttpGet("list-vw")]
        //public async Task<IActionResult> ListVwProducts([FromQuery] string code)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(code))
        //            return BadRequest("code param needed");

        //        // 1) Tüm Vw ürünlerini parça parça çek (limit=100)
        //        int vwCategoryId = 7; // Sizin panelinizde VW ID farklıysa değiştirin
        //        var allProducts = await _ideaSoftService.GetAllProductsForCategory(vwCategoryId, code, 100);

        //        // 2) Bellekteki ürün sayısı
        //        int totalCount = allProducts.Count;
        //        if (totalCount == 0)
        //            return Ok("No VW products found for this category.");

        //        // 3) Tek seferde dosyaya yaz
        //        var filePath = Path.Combine(_env.ContentRootPath, "Product-List/vw-all.json");
        //        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        //        var options = new JsonSerializerOptions { WriteIndented = true };
        //        var jsonString = JsonSerializer.Serialize(allProducts, options);

        //        await System.IO.File.WriteAllTextAsync(filePath, jsonString);

        //        return Ok($"Found {totalCount} VW products. Saved to {filePath}");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[HttpDelete("delete-all-vw-products")]
        //public async Task<IActionResult> DeleteAllFiatProducts([FromQuery] string code)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(code))
        //            return BadRequest("code parameter is required.");

        //        // 1) Access token alın (varsa yeniden token alabilirsiniz)
        //        await _ideaSoftService.GetAccessTokenAsync(code);

        //        // 2) "fiat-all.json" dosyasını oku (Product-List klasörü içinde)
        //        var filePath = Path.Combine(_env.ContentRootPath, "Product-List/vw-all.json");
        //        if (!System.IO.File.Exists(filePath))
        //            return BadRequest("vw-all.json not found.");

        //        var jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
        //        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        //        var productList = JsonSerializer.Deserialize<List<IdeaSoftListingProductDto>>(jsonContent, options);

        //        if (productList == null || productList.Count == 0)
        //            return BadRequest("vw-all.json is empty.");

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

        //        return Ok($"Deleted {deletedCount} VW products.");
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
