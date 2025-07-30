using System.Text.Json.Serialization;
namespace backend.Models.Entities.Tiktok;

public class TikTokVideo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("embed_link")]
    public string EmbedLink { get; set; } = string.Empty;
}

public class TikTokVideoListResponse
{
    [JsonPropertyName("data")]
    public VideoListData? Data { get; set; }
}

public class VideoListData
{
    [JsonPropertyName("videos")]
    public List<TikTokVideo> Videos { get; set; } = [];
}