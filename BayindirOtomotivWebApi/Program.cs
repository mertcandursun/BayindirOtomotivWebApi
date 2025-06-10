using BayindirOtomotivWebApi.Controllers;
using BayindirOtomotivWebApi.Helpers;
using BayindirOtomotivWebApi.Infrastructure;
using BayindirOtomotivWebApi.Jobs;
using BayindirOtomotivWebApi.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BayindirDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHttpClient("BasbugClient", client =>
{
    client.BaseAddress = new Uri("https://api.basbug.com.tr");
});

builder.Services.AddHttpClient("IdeaSoftClient", client =>
{
    client.BaseAddress = new Uri("https://bayindirotomotiv.myideasoft.com");
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient("TecDocClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};
var httpClient = new HttpClient(handler);



// DI kayýtlarý
// IdeaSoftService
builder.Services.AddScoped<BasbugService>();
builder.Services.AddScoped<GoogleImageService>();
builder.Services.AddScoped<TecDocService>();

builder.Services.AddScoped<IdeaSoftTokenStore>();
builder.Services.RemoveAll(typeof(IdeaSoftService));   // Microsoft.Extensions.DependencyInjection.Abstractions
builder.Services.AddScoped<IdeaSoftService>();

// Hangfire
// Hangfire – SQL Server storage
builder.Services.AddHangfire(cfg =>
{
    cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
       .UseSimpleAssemblyNameTypeSerializer()
       .UseRecommendedSerializerSettings()
       .UseSqlServerStorage(builder.Configuration.GetConnectionString("Default"),
            new Hangfire.SqlServer.SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.FromSeconds(15),
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            });
});
builder.Services.AddHangfireServer();

builder.Services.AddTransient<OpelIntegrationController>();

builder.Services.AddTransient<OpelSyncJob>();

builder.Services.AddTransient<FiatIntegrationController>();

builder.Services.AddTransient<FiatSyncJob>();

builder.Services.AddTransient<FordIntegrationController>();

builder.Services.AddTransient<FordSyncJob>();

builder.Services.AddTransient<VWIntegrationController>();

builder.Services.AddTransient<VWSyncJob>();

builder.Services.AddTransient<PSAIntegrationController>();

builder.Services.AddTransient<PSASyncJob>();

builder.Services.AddTransient<RenaultIntegrationController>();

builder.Services.AddTransient<RenaultSyncJob>();

builder.Services.AddTransient<BrandController>();

builder.Services.AddTransient<LightingSyncJob>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.UseHttpsRedirection();

app.UseHangfireDashboard("/jobs");

RecurringJob.AddOrUpdate<IdeaSoftService>(
    "ideasoft-token-refresh",
    service => service.RefreshTokenIfNeededAsync(),
    Cron.HourInterval(8));





RecurringJob.AddOrUpdate<OpelSyncJob>(
    "opel-full-sync",
    j => j.RunAsync(),
    "0 2 * * *",
    TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

RecurringJob.AddOrUpdate<FiatSyncJob>(
    "fiat-full-sync",
    j => j.RunAsync(),
    "0 4 * * *",
    TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

RecurringJob.AddOrUpdate<FordSyncJob>(
    "ford-full-sync",
    j => j.RunAsync(),
    "0 3 * * *",
    TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

RecurringJob.AddOrUpdate<VWSyncJob>(
    "vw-full-sync",
    j => j.RunAsync(),
    "0 3 * * *",
    TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

RecurringJob.AddOrUpdate<PSASyncJob>(
    "psa-full-sync",
    j => j.RunAsync(),
    "0 2 * * *",
    TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

RecurringJob.AddOrUpdate<RenaultSyncJob>(
    "renault-full-sync",
    j => j.RunAsync(),
    "0 4 * * *",
    TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

RecurringJob.AddOrUpdate<LightingSyncJob>(
    "lighting-full-sync",
    j => j.RunAsync(),
    "0 5 * * *",
    TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time"));

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BayindirDbContext>();
    db.Database.Migrate();
}

//using (var scope = app.Services.CreateScope())
//{
//    var ideaSoft = scope.ServiceProvider.GetRequiredService<IdeaSoftService>();
//    var cats = await ideaSoft.GetAllCategoriesAsync();
//    GlobalCategoryCache.Init(cats);
//}

app.Run();