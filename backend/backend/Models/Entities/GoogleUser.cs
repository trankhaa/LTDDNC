// backend/Models/Entities/User.cs
using System;
using System.ComponentModel.DataAnnotations; // Vẫn có thể dùng cho validation ở DTOs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models.Entities
{
    public class GoogleUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("email")]
        // [Required] // Email vẫn nên required
        public string Email { get; set; } = string.Empty;

        [BsonElement("password")]
        public string? Password { get; set; } // Cho phép null vì Google login không có pass

        [BsonElement("fullName")] // Thêm để lưu tên từ Google
        public string? FullName { get; set; }

        [BsonElement("googleId")] // Để lưu ID duy nhất từ Google
        public string? GoogleId { get; set; }

        [BsonElement("profilePictureUrl")] // Thêm để lưu ảnh đại diện từ Google
        public string? ProfilePictureUrl { get; set; }

        [BsonElement("loginProvider")] // "Local", "Google"
        public string LoginProvider { get; set; } = "Local";

        [BsonElement("role")]
        public string Role { get; set; } = "Patient"; // "Patient", "Doctor", "Admin"

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("doctorId")]
        public string? DoctorId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("patientId")]
        public string? PatientId { get; set; }

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}