// File: backend/Models/Entities/Chat/ChatMessage.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models.Entities.Chat
{
    public class ChatMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("FromUser")]
        public required string From { get; set; }

        [BsonElement("ToUser")]
        public required string To { get; set; }

        [BsonElement("Message")]
        public required string Message { get; set; }

        [BsonElement("Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}