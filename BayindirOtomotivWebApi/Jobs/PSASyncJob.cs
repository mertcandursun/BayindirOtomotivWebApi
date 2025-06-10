using BayindirOtomotivWebApi.Controllers;
using Hangfire;

namespace BayindirOtomotivWebApi.Jobs
{
    public class PSASyncJob
    {
        private readonly PSAIntegrationController _ctrl;
        private readonly IdeaSoftTokenStore _tokenStore;

        public PSASyncJob(PSAIntegrationController ctrl, IdeaSoftTokenStore tokenStore)
        {
            _ctrl = ctrl;
            _tokenStore = tokenStore;
        }

        [AutomaticRetry]
        public async Task RunAsync()
        {
            // Get PSA materials to Basbug
            await _ctrl.FetchAndSavePSAMaterials();

            // Stock(Basbug) + Images(Tecdoc)
            await _ctrl.FetchStocksAndImagesPsa();

            // Ideasoft UPDATE/CREATE
            var tokens = await _tokenStore.LoadAsync();
            if (tokens is null)
            {
                throw new Exception("Ideasoft token not found");
            }

            await _ctrl.UpdatePsaProductsWaiting();

            await _ctrl.CreatePsaProducts();
        }
    }
}
