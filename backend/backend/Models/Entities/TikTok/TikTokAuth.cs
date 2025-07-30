namespace backend.Models.Entities.Tiktok;

public class TikTokSettings
{
    public string ClientKey { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}

public class UserToken
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
    public string OpenId { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}