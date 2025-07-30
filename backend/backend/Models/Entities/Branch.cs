using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities
{
    public class Branch
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        // Thuộc tính [BsonElement] không cần thiết cho _id, vì [BsonId] đã xử lý việc này.
        public string IdBranch { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("branchName")]
        [Required(ErrorMessage = "Branch name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string BranchName { get; set; } = string.Empty;

        [BsonElement("branchAddress")]
        [Required(ErrorMessage = "Address is required")]
        public string BranchAddress { get; set; } = string.Empty;

        [BsonElement("branchHotline")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string BranchHotline { get; set; } = string.Empty;

        // --- CÁC TRƯỜNG ĐƯỢC BỔ SUNG ---

        [BsonElement("branchEmail")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string BranchEmail { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("coordinates")]
        public GeoPoint? Coordinates { get; set; } // Lưu tọa độ (latitude, longitude), có thể null

        // --- KẾT THÚC PHẦN BỔ SUNG ---

        [BsonElement("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Lớp để lưu trữ tọa độ địa lý.
    /// Cần thiết cho thuộc tính 'Coordinates'.
    /// </summary>
    public class GeoPoint
    {
        [BsonElement("latitude")]
        public double Latitude { get; set; }

        [BsonElement("longitude")]
        public double Longitude { get; set; }
    }
}