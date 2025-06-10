// Jobs/OpelSyncJob.cs
using BayindirOtomotivWebApi.Controllers;
using BayindirOtomotivWebApi.Infrastructure;   // TokenStore
using Hangfire;
using Microsoft.AspNetCore.Mvc;

public class OpelSyncJob
{
    private readonly OpelIntegrationController _ctrl;
    private readonly IdeaSoftTokenStore _tokenStore;

    public OpelSyncJob(OpelIntegrationController ctrl,
                       IdeaSoftTokenStore tokenStore)
    {
        _ctrl = ctrl;
        _tokenStore = tokenStore;
    }

    [AutomaticRetry(Attempts = 0)]            // hata durumunda yeniden deneme yok
    public async Task RunAsync()
    {
        /* 1) BASBUG → OPEL malzemeleri */
        await _ctrl.FetchAndSaveOpelMaterials();

        /* 2) Stok + TecDoc görüntüleri  */
        await _ctrl.FetchStocksAndImagesOpel();

        /* 3) IdeaSoft UPDATE/CREATE     */
        var tokens = await _tokenStore.LoadAsync();
        if (tokens is null)
            throw new Exception("IdeaSoft token bilgisi bulunamadı.");

        await _ctrl.UpdateOpelProductsWaiting();

        await _ctrl.CreateOpelProducts();
    }
}
