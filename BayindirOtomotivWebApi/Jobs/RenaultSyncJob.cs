using BayindirOtomotivWebApi.Controllers;
using Hangfire;

namespace BayindirOtomotivWebApi.Jobs
{
    public class RenaultSyncJob
    {
        private readonly RenaultIntegrationController _ctrl;
        private readonly IdeaSoftTokenStore _tokenStore;

        public RenaultSyncJob(RenaultIntegrationController ctrl, IdeaSoftTokenStore tokenStore)
        {
            _ctrl = ctrl;
            _tokenStore = tokenStore;
        }

        [AutomaticRetry]
        public async Task RunAsync()
        {
            // Get Renault materials to Basbug
            await _ctrl.FetchAndSaveRenaultMaterials();

            // Stock(Basbug) + Images(Tecdoc)
            await _ctrl.FetchStocksAndImagesRenault();

            // Ideasoft UPDATE/CREATE
            var tokens = await _tokenStore.LoadAsync();
            if (tokens is null)
            {
                throw new Exception("Ideasoft token not found");
            }

            await _ctrl.UpdateRenaultProductsAsync();

            await _ctrl.CreateRenaultProductsAsync();
        }
    }
}
