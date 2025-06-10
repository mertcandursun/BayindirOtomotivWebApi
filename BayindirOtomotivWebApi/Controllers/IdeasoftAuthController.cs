using BayindirOtomotivWebApi.Infrastructure;
using BayindirOtomotivWebApi.Models;
using BayindirOtomotivWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BayindirOtomotivWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdeaSoftController : ControllerBase
    {
        private readonly IdeaSoftService _ideaSoftService;
        private readonly IWebHostEnvironment _env;

        public IdeaSoftController(IdeaSoftService ideaSoftService, IWebHostEnvironment env)
        {
            _ideaSoftService = ideaSoftService;
            _env = env;
        }

        /// <summary>
        /// 1) Auth Code almak için tarayıcıda açmanız gereken URL’yi döndürür
        /// </summary>
        //[HttpGet("getAuthCodeUrl")]
        //public IActionResult GetAuthCodeUrl()
        //{
        //    var url = _ideaSoftService.GetAuthCodeUrl();
        //    return Ok(new { AuthCodeUrl = url });
        //}

        // Api/AuthController.cs
        [HttpPost("ideasoft/initialize")]
        public async Task<IActionResult> InitializeIdeaSoft([FromQuery] string code,
                                                            [FromServices] IdeaSoftService idea,
                                                            [FromServices] IdeaSoftTokenStore store)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest("code gerekli");

            await idea.AcquireInitialTokensAsync(code);   // dosyaya yazar
            return Ok("Token kaydedildi, Hangfire job'u artık çalışabilir.");
        }

        [HttpGet("token")]
        public async Task<IActionResult> GetAccessToken()
        {
            try
            {
                var token = await _ideaSoftService.GetAccessTokenAsync();
                return Ok(new { AccessToken = token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("ideasoft-categories")]
        public async Task<IActionResult> GetIdeaSoftCategories([FromQuery] string? code = null)
        {
            try
            {
                await EnsureIdeaSoftAuthAsync(code);

                // 1) Tüm kategorileri al
                var cats = await _ideaSoftService.GetAllCategoriesAsync();

                // 2) Disk’e yedekle
                var filePath = Path.Combine(_env.ContentRootPath, "Product-List", "ideasoft-categories.json");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                await System.IO.File.WriteAllTextAsync(
                    filePath,
                    JsonSerializer.Serialize(cats, new JsonSerializerOptions { WriteIndented = true }));

                // 3) Sonuç
                return Ok(new
                {
                    count = cats.Count,
                    filePath = filePath
                });
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


        /// <summary>
        /// 2) Redirect sonrası gelen "code" parametresiyle token alır
        /// Örnek çağrı: GET /api/IdeaSoftAuth/step2-getToken?code=XYZ
        /// </summary>
        //[HttpGet("step2-getToken")]
        //public async Task<IActionResult> GetToken([FromQuery] string code)
        //{
        //    if (string.IsNullOrWhiteSpace(code))
        //        return BadRequest("code parametresi gerekli.");

        //    var token = await _ideaSoftService.GetAccessTokenAsync(code);
        //    return Ok(new { AccessToken = token });
        //}

        //[HttpGet("list-categories")]
        //public async Task<IActionResult> ListAllCategories([FromQuery] string code)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(code))
        //            return BadRequest("code param needed.");

        //        int sinceId = 1;
        //        var allCategories = await _ideaSoftService.GetAllCategories(sinceId, code, 100);

        //        // 2) Bellekteki ürün sayısı
        //        int totalCount = allCategories.Count;
        //        if (totalCount == 0)
        //            return Ok("No Opel products found for this category.");

        //        // 3) Tek seferde dosyaya yaz
        //        var filePath = Path.Combine(_env.ContentRootPath, "Category-List/all-categories.json");
        //        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        //        var options = new JsonSerializerOptions { WriteIndented = true };
        //        var jsonString = JsonSerializer.Serialize(allCategories, options);

        //        await System.IO.File.WriteAllTextAsync(filePath, jsonString);

        //        return Ok($"Found {totalCount} Category. Saved to {filePath}");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}
