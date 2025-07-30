using backend.Models.Entities.Chat;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using backend.Settings;

namespace backend.Services.Chat
{
    public class ChatService : IChatService
    {
        private readonly IMongoCollection<ChatMessage> _messagesCollection;

        public ChatService(IMongoDatabase database, IOptions<MongoDbSettings> settings)
        {
            var collectionName = settings.Value.Collections?.ChatMessages ?? "ChatMessages";
            _messagesCollection = database.GetCollection<ChatMessage>(collectionName);
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            await _messagesCollection.InsertOneAsync(message);
        }

        public async Task<List<ChatMessage>> GetAllMessagesAsync()
        {
            return await _messagesCollection.Find(_ => true).SortBy(m => m.Timestamp).ToListAsync();
        }

        // --- TRIỂN KHAI PHƯƠNG THỨC MỚI ---
        public async Task<List<ChatMessage>> GetMessagesForUserAsync(string userId)
        {
            // Tìm tất cả tin nhắn CÓ LIÊN QUAN đến userId (hoặc là người gửi, hoặc là người nhận)
            // Trong trường hợp này, chat với 'admin'
            var filter = Builders<ChatMessage>.Filter.Or(
                Builders<ChatMessage>.Filter.Eq(m => m.From, userId),
                Builders<ChatMessage>.Filter.Eq(m => m.To, userId)
            );

            return await _messagesCollection.Find(filter).SortBy(m => m.Timestamp).ToListAsync();
        }
    }
}