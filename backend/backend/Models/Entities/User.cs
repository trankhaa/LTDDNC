using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string GoogleId { get; set; } // Hoặc kiểu dữ liệu phù hợp

        [BsonElement("email")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Required]
        public string Email { get; set; } = string.Empty; public string? Name { get; set; } // ✅ Cần thêm dòng này

        [BsonElement("password")]
        [Required]
        public string Password { get; set; } = string.Empty;

        [BsonElement("role")]
        public string Role { get; set; } = "Patient"; // "Patient", "Doctor", "Admin"

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        // Liên kết với các bảng khác
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

    public class Userlogin
    {
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

        // Optional: login bằng phone nếu muốn
        public string? Phone { get; set; }
    }

    public enum UserGender
    {
        nam,
        nu,
        other
    }
}