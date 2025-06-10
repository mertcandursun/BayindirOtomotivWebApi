using BayindirOtomotivWebApi.Helpers;
using BayindirOtomotivWebApi.Models.Basbug;
using BayindirOtomotivWebApi.Models;
using BayindirOtomotivWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BayindirOtomotivWebApi.OldController
{
    public class OldOpel
    {
        //[HttpGet("fetch-stocks-opel")]
        //public async Task<IActionResult> FetchStocksForOpelMaterials()
        //{
        //    try
        //    {
        //        // 1) Basbug Auth
        //        await _basbugService.AuthenticateAsync();

        //        // 1) "opel-materials.json" oku
        //        var filePath = Path.Combine(_env.ContentRootPath, "raw_Outputs/opel-materials.json");
        //        if (!System.IO.File.Exists(filePath))
        //            return BadRequest("opel-materials.json not found.");

        //        var jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
        //        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        //        var allOpelMaterials = JsonSerializer.Deserialize<List<MalzemeDto>>(jsonContent, options);

        //        if (allOpelMaterials == null || allOpelMaterials.Count == 0)
        //            return BadRequest("opel-materials.json is empty.");

        //        // noList -> 30k adet no
        //        List<string> noList = allOpelMaterials.Select(m => m.no).Distinct().ToList();

        //        // chunkResults -> tüm chunk’ların sonucunu toplayacağız
        //        var allResults = new List<MalzemeAraDto>();
        //        var count = allResults.ToList();

        //        // chunk boyutu, 200-500 gibi
        //        int chunkSize = 500;

        //        for (int i = 0; i < noList.Count; i += chunkSize)
        //        {
        //            var subset = noList.Skip(i).Take(chunkSize).ToList();
        //            // virgül birleştir
        //            var joinedNos = string.Join(",", subset);

        //            // TopluMalzemeAra çağrısı
        //            var partialResults = await _basbugService.GetStockInfoByTopluMalzemeAra(joinedNos);

        //            // partialResults’u allResults’a ekliyoruz
        //            allResults.AddRange(partialResults);
        //        }

        //        // Şimdi allResults içinde 30k no’nun stok bilgisi var
        //        // Stok >= 1 olanları filtreleyelim
        //        var inStockItems = allResults
        //            .Where(x => (x.sMrk + x.sIzm + x.sAnk + x.sAdn + x.sErz) >= 1)
        //            .ToList();

        //        // Eğer base64 resim ekleyecekseniz, isterseniz yine chunk chunk yapabilirsiniz.
        //        // Yine de 30k item için Google API limiti, kota gibi konulara dikkat etmek gerek
        //        //foreach (var item in inStockItems)
        //        //{
        //        //    var base64 = await _googleImageService.DownloadImageAsBase64Async(item.oe);
        //        //    if (!string.IsNullOrEmpty(base64))
        //        //    {
        //        //        item.imgBase64 = base64;
        //        //    }
        //        //}

        //        // Son olarak JSON’a kaydedin
        //        var newFilePath = Path.Combine(_env.ContentRootPath, "Outputs/opel-instock-all.json");
        //        var newJson = JsonSerializer.Serialize(inStockItems, new JsonSerializerOptions { WriteIndented = true });
        //        await System.IO.File.WriteAllTextAsync(newFilePath, newJson);

        //        return Ok(new { total = inStockItems.Count, newFilePath });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        //[HttpGet("update-tecdoc-images-opel")]
        //public async Task<IActionResult> UpdateImagesFromTecDoc()
        //{
        //    try
        //    {
        //        // Step 1: Read the "opel-instock-all.json" file
        //        var filePath = Path.Combine(_env.ContentRootPath, "Outputs/opel-instock-all.json");
        //        if (!System.IO.File.Exists(filePath))
        //            return BadRequest("opel-instock-all.json not found.");

        //        var jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
        //        var options = new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true,
        //            WriteIndented = true
        //        };

        //        // Deserialize the JSON into the product list.
        //        var productList = JsonSerializer.Deserialize<List<MalzemeAraDto>>(jsonContent, options);
        //        if (productList == null || productList.Count == 0)
        //            return BadRequest("opel-instock-all.json is empty.");

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
        //                var tecDocResponse = await _tecDocService.SearchArticleOEMAsync(firstOe);
        //                if (tecDocResponse?.articles != null && tecDocResponse.articles.Count > 0)
        //                {
        //                    // Get the s3ImageLink from the first article in the response.
        //                    var imageUrl = tecDocResponse.articles[0].s3ImageLink;
        //                    // Download the image from the URL and convert it to Base64.
        //                    var base64Image = await _tecDocService.DownloadImageAsBase64FromUrl(imageUrl);
        //                    if (!string.IsNullOrEmpty(base64Image))
        //                    {
        //                        product.imgBase64 = base64Image;
        //                    }
        //                    else
        //                    {
        //                        // If conversion fails (empty result), add to failed list.
        //                        failedProducts.Add(product);
        //                    }
        //                }
        //                else
        //                {
        //                    // If no articles were returned, add product to failed list.
        //                    failedProducts.Add(product);
        //                }
        //            }
        //            catch (HttpRequestException ex)
        //            {
        //                // Log the exception and add the product to the failed list.
        //                Console.WriteLine($"Request for product {product.oe} failed: {ex.Message}");
        //                failedProducts.Add(product);
        //            }

        //            // Delay 1 second between requests to reduce rate limit issues.
        //            await Task.Delay(200);
        //        }

        //        // Step 3: Write the updated product list to a JSON file
        //        var newFilePath = Path.Combine(_env.ContentRootPath, "Outputs/opel-instock-all-updated.json");
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
        //            var failedFilePath = Path.Combine(failedFolder, $"opel-fail-pictures-{today}.json");
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

        //[HttpPut("update-opel")]
        //public async Task<IActionResult> UpdateOpelProducts([FromQuery] string? code = null)
        //{
        //    try
        //    {
        //        await EnsureIdeaSoftAuthAsync(code);

        //        int opelCategoryId = 1;
        //        var allProducts = await _ideaSoftService.GetAllProductsForCategory(opelCategoryId, 100);

        //        if (allProducts.Count == 0)
        //            return Ok("No Opel products found for this category.");

        //        var filePath = Path.Combine(_env.ContentRootPath, "Product-List/opel-all.json");
        //        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        //        await System.IO.File.WriteAllTextAsync(
        //            filePath,
        //            JsonSerializer.Serialize(allProducts, new JsonSerializerOptions { WriteIndented = true }));

        //        var listPath = Path.Combine(_env.ContentRootPath, "Product-List/opel-all.json");
        //        var srcPath = Path.Combine(_env.ContentRootPath, "Outputs/opel-instock-all-updated.json");

        //        if (!System.IO.File.Exists(listPath) || !System.IO.File.Exists(srcPath))
        //            return BadRequest("JSON files not found.");

        //        var ideaList = JsonSerializer.Deserialize<List<IdeaSoftListingProductDto>>(
        //                            await System.IO.File.ReadAllTextAsync(listPath),
        //                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        //        var srcItems = JsonSerializer.Deserialize<List<MalzemeAraDto>>(
        //                            await System.IO.File.ReadAllTextAsync(srcPath),
        //                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        //        string Norm(string? s) => s?
        //            .Replace("/", " ").Replace("-", " ")
        //            .Replace("  ", " ").Trim().ToUpperInvariant() ?? "";

        //        var ideaDict = ideaList.ToDictionary(p => Norm(p.sku), p => p, StringComparer.Ordinal);
        //        var srcDict = srcItems.ToDictionary(p => Norm(p.no), p => p, StringComparer.Ordinal);

        //        int updated = 0;
        //        foreach (var kv in ideaDict)
        //        {
        //            if (!srcDict.TryGetValue(kv.Key, out var src)) continue;

        //            var dto = IdeasoftMapper.UpdateMapToIdeaSoftDto(src);
        //            var resp = await _ideaSoftService.UpdateProductAsync(kv.Value.id, dto);

        //            if (resp.IsSuccess) updated++;
        //            await Task.Delay(300);
        //        }

        //        var toCreate = srcItems.Where(s => !ideaDict.ContainsKey(Norm(s.no))).ToList();

        //        int created = 0;
        //        foreach (var src in toCreate)
        //        {
        //            var dto = IdeasoftMapper.MapToIdeaSoftDto(src);
        //            var resp = await _ideaSoftService.CreateProductAsync(dto);
        //            if (resp.IsSuccess) created++;
        //            await Task.Delay(300);
        //        }

        //        if (toCreate.Count > 0)
        //        {
        //            var missPath = Path.Combine(_env.ContentRootPath,
        //                            $"Missing-Update/opel-{DateTime.Now:yyyyMMdd-HHmm}.json");
        //            Directory.CreateDirectory(Path.GetDirectoryName(missPath)!);

        //            await System.IO.File.WriteAllTextAsync(
        //                missPath,
        //                JsonSerializer.Serialize(toCreate, new JsonSerializerOptions { WriteIndented = true }));
        //        }

        //        return Ok($"Update completed | Updated: {updated} | Created: {created} | Missing: {toCreate.Count - created}");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}
