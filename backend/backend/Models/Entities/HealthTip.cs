// File: Models/Entities/HealthTip.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models.Entities
{
    public class HealthTip
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("shortDescription")] // Mô tả ngắn để hiển thị ở danh sách
        public string ShortDescription { get; set; } = string.Empty;

        [BsonElement("content")] // Nội dung đầy đủ cho trang chi tiết
        public string Content { get; set; } = string.Empty;

        [BsonElement("category")] // VD: "Dinh dưỡng", "Thể chất", "Tinh thần"
        public string Category { get; set; } = string.Empty;

        [BsonElement("imageUrl")] // URL ảnh minh họa (có thể null)
        public string? ImageUrl { get; set; }

        [BsonElement("source")] // Nguồn tham khảo (VD: "Theo Viện Dinh Dưỡng")
        public string? Source { get; set; }

        [BsonElement("isFeatured")] // Đánh dấu để hiển thị ở trang chủ
        public bool IsFeatured { get; set; } = false;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}