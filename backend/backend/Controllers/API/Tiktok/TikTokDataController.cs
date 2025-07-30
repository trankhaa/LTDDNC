using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using backend.Models.Entities.Tiktok;
using backend.Services.Tiktok;

namespace backend.Controllers.API.Tiktok;
[ApiController]
[Route("api/tiktok")]
public class TikTokDataController : ControllerBase
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly TokenStorageService _tokenStorage;

    public TikTokDataController(IHttpClientFactory cf, TokenStorageService ts)
    {
        _clientFactory = cf;
        _tokenStorage = ts;
    }

    [HttpGet("videos")]
    public async Task<IActionResult> GetVideos()
    {
        var token = await _tokenStorage.GetTokenAsync();
        if (token == null || token.AccessTokenExpiresAt <= DateTime.UtcNow)
        {
            return Unauthorized("Token không hợp lệ hoặc đã hết hạn. Vui lòng kết nối lại.");
        }

        var client = _clientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        // Lấy các trường cần thiết, đặc biệt là 'embed_link'
        var fields = "id,title,embed_link";
        var requestUrl = $"https://open.tiktokapis.com/v2/video/list/?fields={fields}";

        // API /video/list yêu cầu phương thức POST với body là JSON trống
        var response = await client.PostAsync(requestUrl, new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "Lỗi khi lấy video từ TikTok: " + await response.Content.ReadAsStringAsync());
        }

        var videoData = await response.Content.ReadFromJsonAsync<TikTokVideoListResponse>();
        return Ok(videoData?.Data);
    }
}