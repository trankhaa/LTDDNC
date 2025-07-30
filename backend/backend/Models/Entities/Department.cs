using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace backend.Models.Entities
{
    public class Department
    {
        [BsonId]
        [BsonElement("IdDepartment")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdDepartment { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("DepartmentName")]
        public string DepartmentName { get; set; } = string.Empty;

        [BsonElement("Description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("CreatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("UpdatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("ImageUrl")]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
