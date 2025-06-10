using BayindirOtomotivWebApi.Helpers;
using BayindirOtomotivWebApi.Models.Basbug;
using BayindirOtomotivWebApi.Models;
using BayindirOtomotivWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BayindirOtomotivWebApi.OldController
{
    public class OldRenault
    {
        //[HttpPut("update-renault")]
        //public async Task<IActionResult> UpdateRenaultProducts([FromQuery] string? code = null)
        //{
        //    try
        //    {
        //        await EnsureIdeaSoftAuthAsync(code);

        //        int renaultCategoryId = 5;
        //        var allProducts = await _ideaSoftService.GetAllProductsForCategory(renaultCategoryId, 100);

        //        if (allProducts.Count == 0)
        //            return Ok("No Renault products found for this category.");

        //        var filePath = Path.Combine(_env.ContentRootPath, "Product-List/renault-all.json");
        //        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        //        await System.IO.File.WriteAllTextAsync(
        //            filePath,
        //            JsonSerializer.Serialize(allProducts, new JsonSerializerOptions { WriteIndented = true }));

        //        var listPath = Path.Combine(_env.ContentRootPath, "Product-List/renault-all.json");
        //        var srcPath = Path.Combine(_env.ContentRootPath, "Outputs/renault-instock-all-updated.json");

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
        //            await Task.Delay(350);
        //        }

        //        var toCreate = srcItems.Where(s => !ideaDict.ContainsKey(Norm(s.no))).ToList();

        //        int created = 0;
        //        foreach (var src in toCreate)
        //        {
        //            var dto = IdeasoftMapper.MapToIdeaSoftDto(src);
        //            var resp = await _ideaSoftService.CreateProductAsync(dto);
        //            if (resp.IsSuccess) created++;
        //            await Task.Delay(350);
        //        }

        //        if (toCreate.Count > 0)
        //        {
        //            var missPath = Path.Combine(_env.ContentRootPath,
        //                            $"Missing-Update/renault-{DateTime.Now:yyyyMMdd-HHmm}.json");
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
