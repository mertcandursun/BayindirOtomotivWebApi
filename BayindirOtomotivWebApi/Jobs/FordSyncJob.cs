using BayindirOtomotivWebApi.Controllers;
using Hangfire;

namespace BayindirOtomotivWebApi.Jobs
{
    public class FordSyncJob
    {
        private readonly FordIntegrationController _ctrl;
        private readonly IdeaSoftTokenStore _tokenStore;

        public FordSyncJob(FordIntegrationController ctrl, IdeaSoftTokenStore tokenStore)
        {
            _ctrl = ctrl;
            _tokenStore = tokenStore;
        }

        [AutomaticRetry]
        public async Task RunAsync()
        {
            // Get Ford materials to Basbug
            await _ctrl.FetchAndSaveFordMaterials();

            // Stock(Basbug) + Images(Tecdoc)
            await _ctrl.FetchStocksAndImagesFord();

            // Ideasoft UPDATE/CREATE
            var tokens = await _tokenStore.LoadAsync();
            if (tokens is null)
            {
                throw new Exception("Ideasoft token not found");
            }

            await _ctrl.UpdateFordProductsWaiting();

            await _ctrl.CreateFordProducts();
        }
    }
}
