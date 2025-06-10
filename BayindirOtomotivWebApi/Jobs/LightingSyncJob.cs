using BayindirOtomotivWebApi.Controllers;
using Hangfire;

namespace BayindirOtomotivWebApi.Jobs
{
    public class LightingSyncJob
    {
        private readonly BrandController _ctrl;
        private readonly IdeaSoftTokenStore _tokenStore;

        public LightingSyncJob(BrandController ctrl, IdeaSoftTokenStore tokenStore)
        {
            _ctrl = ctrl;
            _tokenStore = tokenStore;
        }

        [AutomaticRetry]
        public async Task RunAsync()
        {
            // Get Lighting materials to Basbug
            await _ctrl.FetchLightFiles();

            // Stock(Basbug) + Images(Tecdoc)
            await _ctrl.FetchStocksAndImagesAll();

            // Ideasoft UPDATE/CREATE
            var tokens = await _tokenStore.LoadAsync();
            if (tokens is null)
            {
                throw new Exception("Ideasoft token not found");
            }

            await _ctrl.UpdateLightingProductsWaiting();

            await _ctrl.CreateLightingProducts();
        }
    }
}
