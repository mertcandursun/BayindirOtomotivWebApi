namespace BayindirOtomotivWebApi.Infrastructure;

public class TokenInfo
{
    public int Id { get; set; }
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
}