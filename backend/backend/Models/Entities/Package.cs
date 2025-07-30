// file: Models/Entities/Package.cs

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace backend.Models.Entities
{
    public class Package
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Description")]
        public string Description { get; set; }

        [BsonElement("ImageUrl")]
        public string ImageUrl { get; set; } // URL hình ảnh minh họa cho gói khám

        [BsonElement("Price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; } // Giá sau khi đã giảm (nếu có)

        [BsonElement("OriginalPrice")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal? OriginalPrice { get; set; } // Giá gốc (để hiển thị gạch ngang)

        [BsonElement("ItemsIncluded")]
        public List<string> ItemsIncluded { get; set; } // Danh sách các mục khám trong gói

        [BsonElement("IsActive")]
        public bool IsActive { get; set; } // Để bật/tắt gói khám

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}