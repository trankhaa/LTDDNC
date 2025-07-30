using backend.Models.Entities.Tiktok;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace backend.Services.Tiktok;

// Lưu ý: Đây là cách lưu đơn giản vào file, không an toàn cho production.
// Trong thực tế, bạn nên lưu vào Database.
public class TokenStorageService
{
    private const string TokenFilePath = "tiktok_token.json";

    public async Task SaveTokenAsync(UserToken token)
    {
        var json = JsonSerializer.Serialize(token);
        await File.WriteAllTextAsync(TokenFilePath, json);
    }

    public async Task<UserToken?> GetTokenAsync()
    {
        if (!File.Exists(TokenFilePath)) return null;
        var json = await File.ReadAllTextAsync(TokenFilePath);
        return JsonSerializer.Deserialize<UserToken>(json);
    }
}