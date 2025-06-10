using BayindirOtomotivWebApi.Controllers;
using Hangfire;

namespace BayindirOtomotivWebApi.Jobs
{
    public class FiatSyncJob
    {
        private readonly FiatIntegrationController _ctrl;
        private readonly IdeaSoftTokenStore _tokenStore;

        public FiatSyncJob(FiatIntegrationController ctrl, IdeaSoftTokenStore tokenStore)
        {
            _ctrl = ctrl;
            _tokenStore = tokenStore;
        }

        [AutomaticRetry]
        public async Task RunAsync()
        {
            // Get Fiat materials to Basbug
            await _ctrl.FetchAndSaveFiatMaterials();

            // Stock(Basbug) + Images(Tecdoc)
            await _ctrl.FetchStocksAndImagesFiat();

            // Ideasoft UPDATE/CREATE
            var tokens = await _tokenStore.LoadAsync();
            if (tokens is null)
            {
                throw new Exception("Ideasoft token not found");
            }

            await _ctrl.UpdateFiatProductsWaiting();

            await _ctrl.CreateFiatProducts();
        }
    }
}
