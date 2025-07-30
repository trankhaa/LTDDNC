using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace backend.Models.Entities
{
    public class Specialty
    {
        [BsonId]
        [BsonElement("IdSpecialty")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdSpecialty { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("SpecialtyName")]
        public string SpecialtyName { get; set; } = string.Empty;

        [BsonElement("Description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("Department")]
        public DepartmentRef? Department { get; set; } // Thay đổi từ DepartmentId sang object Department
        [BsonElement("ImageUrl")]
        public string ImageUrl { get; set; } = string.Empty;

        [BsonElement("CreatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("UpdatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class DepartmentRef
    {
        [BsonElement("IdDepartment")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? IdDepartment { get; set; }

        [BsonElement("DepartmentName")]
        public string? DepartmentName { get; set; }
    }
}