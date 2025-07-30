using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using backend.Models.Entities.Tiktok;
using backend.Services.Tiktok;

namespace backend.Controllers.API.Tiktok;

[ApiController]
[Route("api/auth/tiktok")]
public class AuthController : ControllerBase
{
    private readonly TikTokSettings _settings;
    private readonly IHttpClientFactory _clientFactory;
    private readonly TokenStorageService _tokenStorage;
    private readonly string _frontendUrl;

    public AuthController(IOptions<TikTokSettings> settings, IHttpClientFactory cf, TokenStorageService ts, IConfiguration config)
    {
        _settings = settings.Value;
        _clientFactory = cf;
        _tokenStorage = ts;
        _frontendUrl = config["FrontendUrl"]!;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var scopes = "user.info.basic,video.list";
        var url = $"https://www.tiktok.com/v2/auth/authorize?client_key={_settings.ClientKey}&response_type=code&scope={Uri.EscapeDataString(scopes)}&redirect_uri={Uri.EscapeDataString(_settings.RedirectUri)}&state=your_secure_state";
        return Redirect(url);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        var client = _clientFactory.CreateClient();
        var body = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_key", _settings.ClientKey),
            new KeyValuePair<string, string>("client_secret", _settings.ClientSecret),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("redirect_uri", _settings.RedirectUri),
        });

        var response = await client.PostAsync("https://open.tiktokapis.com/v2/oauth/token/", body);
        if (!response.IsSuccessStatusCode) return BadRequest("Lỗi khi lấy token: " + await response.Content.ReadAsStringAsync());

        var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = jsonDoc.RootElement.GetProperty("data");

        var userToken = new UserToken
        {
            AccessToken = data.GetProperty("access_token").GetString()!,
            RefreshToken = data.GetProperty("refresh_token").GetString()!,
            AccessTokenExpiresAt = DateTime.UtcNow.AddSeconds(data.GetProperty("expires_in").GetInt32()),
            OpenId = data.GetProperty("open_id").GetString()!,
            Scope = data.GetProperty("scope").GetString()!
        };

        await _tokenStorage.SaveTokenAsync(userToken);
        return Redirect(_frontendUrl);
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetAuthStatus()
    {
        var token = await _tokenStorage.GetTokenAsync();
        return Ok(new { isConnected = token != null && token.AccessTokenExpiresAt > DateTime.UtcNow });
    }
}