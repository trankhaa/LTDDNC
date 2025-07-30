using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.Models.Entities.Doctor
{

    public class DoctorDetail
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string IdDoctorDetail { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("doctorId")]
        public string DoctorId { get; set; } = string.Empty;

        [BsonElement("img")]
        public string Img { get; set; } = string.Empty;

        [BsonElement("certificateImg")]
        public string CertificateImg { get; set; } = string.Empty;

        [BsonElement("degreeImg")]
        public string DegreeImg { get; set; } = string.Empty;

        [BsonElement("degree")]
        public string Degree { get; set; } = string.Empty;

        // --- Sửa đổi ở đây ---
        [BsonElement("branchName")] // Tên chi nhánh
        public string BranchName { get; set; } = string.Empty;

        [BsonElement("departmentName")] // Tên khoa
        public string DepartmentName { get; set; } = string.Empty;

        [BsonElement("specialtyName")] // Tên chuyên khoa
        public string SpecialtyName { get; set; } = string.Empty;


        // Các trường string branch, department, specialty
        [BsonElement("branch")]
        public string Branch { get; set; } = string.Empty;

        [BsonElement("department")]
        public string Department { get; set; } = string.Empty;

        [BsonElement("specialty")]
        public string Specialty { get; set; } = string.Empty;

        // Các trường id (có thể là null nếu không có)
        [BsonElement("branchId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? BranchId { get; set; }

        [BsonElement("departmentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? DepartmentId { get; set; }

        [BsonElement("specialtyId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? SpecialtyId { get; set; }

        [BsonElement("description")]
        public string? Description { get; set; }
    }


    public class EducationItem
    {
        [BsonElement("degree")]
        public string Degree { get; set; } = string.Empty;

        [BsonElement("institution")]
        public string Institution { get; set; } = string.Empty;

        [BsonElement("year")]
        public int Year { get; set; }
    }
}