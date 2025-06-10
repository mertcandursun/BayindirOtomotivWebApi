using BayindirOtomotivWebApi.Controllers;
using Hangfire;

namespace BayindirOtomotivWebApi.Jobs
{
    public class VWSyncJob
    {
        private readonly VWIntegrationController _ctrl;
        private readonly IdeaSoftTokenStore _tokenStore;

        public VWSyncJob(VWIntegrationController ctrl, IdeaSoftTokenStore tokenStore)
        {
            _ctrl = ctrl;
            _tokenStore = tokenStore;
        }

        [AutomaticRetry]
        public async Task RunAsync()
        {
            // Get VW materials to Basbug
            await _ctrl.FetchAndSaveVwMaterials();

            // Stock(Basbug) + Images(Tecdoc)
            await _ctrl.FetchStocksAndImagesVw();

            // Ideasoft UPDATE/CREATE
            var tokens = await _tokenStore.LoadAsync();
            if (tokens is null)
            {
                throw new Exception("Ideasoft token not found");
            }

            await _ctrl.UpdateVwProductsWaiting();

            await _ctrl.CreateVwProducts();
        }
    }
}
