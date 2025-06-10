namespace BayindirOtomotivWebApi.OldController
{
    public class OldTecdoc
    {
        //[HttpGet("update-tecdoc-images-ford")]
        //public async Task<IActionResult> UpdateImagesFromTecDoc()
        //{
        //    try
        //    {
        //        // Step 1: Read the "ford-instock-all.json" file
        //        var filePath = Path.Combine(_env.ContentRootPath, "Outputs/ford-instock-all.json");
        //        if (!System.IO.File.Exists(filePath))
        //            return BadRequest("ford-instock-all.json not found.");

        //        var jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
        //        var options = new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true,
        //            WriteIndented = true
        //        };

        //        // Deserialize the JSON into the product list.
        //        var productList = JsonSerializer.Deserialize<List<MalzemeAraDto>>(jsonContent, options);
        //        if (productList == null || productList.Count == 0)
        //            return BadRequest("ford-instock-all.json is empty.");

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
        //                var tecDocResponse = await _tecDocService.SearchArticleOEMAsync(product.oe);
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
        //            await Task.Delay(250);
        //        }

        //        // Step 3: Write the updated product list to a JSON file
        //        var newFilePath = Path.Combine(_env.ContentRootPath, "Outputs/ford-instock-all-updated.json");
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
        //            var failedFilePath = Path.Combine(failedFolder, $"ford-fail-pictures-{today}.json");
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
    }
}
