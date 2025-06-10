using Microsoft.EntityFrameworkCore;
using BayindirOtomotivWebApi.Infrastructure;

public sealed class IdeaSoftTokenStore
{
    private readonly BayindirDbContext _db;

    public IdeaSoftTokenStore(BayindirDbContext db) => _db = db;

    public async Task<TokenInfo?> LoadAsync()
        => await _db.IdeaSoftTokens.AsNoTracking().FirstOrDefaultAsync();

    public async Task SaveAsync(TokenInfo info)
    {
        var existing = await _db.IdeaSoftTokens.FirstOrDefaultAsync();

        if (existing is null)
        {
            // hiç Id set etme, SQL Identity 1’den başlatır
            _db.IdeaSoftTokens.Add(info);
        }
        else
        {
            existing.AccessToken = info.AccessToken;
            existing.RefreshToken = info.RefreshToken;
            existing.ExpiresAtUtc = info.ExpiresAtUtc;
            _db.IdeaSoftTokens.Update(existing);
        }

        await _db.SaveChangesAsync();
    }
}
