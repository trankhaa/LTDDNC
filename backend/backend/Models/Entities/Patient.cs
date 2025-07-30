using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities
{
    public class Patient
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        // Thông tin cá nhân
        [BsonElement("fullName")]
        [Required]
        public string FullName { get; set; } = string.Empty;

        [BsonElement("gender")]
        [BsonRepresentation(BsonType.String)]
        [Required]
        public Gender Gender { get; set; }

        [BsonElement("dateOfBirth")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [Required]
        public DateTime DateOfBirth { get; set; }

        [BsonElement("phone")]
        public string Phone { get; set; } = string.Empty;

        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        // Thông tin y tế
        [BsonElement("patientCode")]
        [Required]
        public string PatientCode { get; set; } = string.Empty;

        [BsonElement("bloodType")]
        public string? BloodType { get; set; } // Nhóm máu (có thể null)

        [BsonElement("height")]
        public double? Height { get; set; } // Chiều cao (cm, có thể null)

        [BsonElement("weight")]
        public double? Weight { get; set; } // Cân nặng (kg, có thể null)

        [BsonElement("insuranceNumber")]
        public string? InsuranceNumber { get; set; } // Số thẻ BHYT (có thể null)
        // Tiền sử bệnh
        [BsonElement("medicalHistory")]
        public List<string>? MedicalHistory { get; set; } // Danh sách tiền sử bệnh (có thể null)

        [BsonElement("allergies")]
        public List<string>? Allergies { get; set; } // Danh sách dị ứng (có thể null)

        // Liên kết với User (nếu có tài khoản)
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("userId")]
        public string? UserId { get; set; }

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum Gender
    {
        Male,
        Female,
        Other
    }
}