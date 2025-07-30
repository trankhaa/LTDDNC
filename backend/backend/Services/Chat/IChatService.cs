using backend.Models.Entities.Chat;

namespace backend.Services.Chat
{
    public interface IChatService
    {
        Task SaveMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetAllMessagesAsync();
        // --- PHƯƠNG THỨC MỚI ---
        Task<List<ChatMessage>> GetMessagesForUserAsync(string userId);
    }
}