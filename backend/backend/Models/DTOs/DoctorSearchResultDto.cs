// DTOs/DoctorSearchResultDto.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using backend.Models.Entities.Doctor; // Chỉ cần dòng này nếu bạn dùng DoctorGender từ đây


namespace backend.Models.DTOs
{
    public class DoctorSearchResultDto
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public DoctorGender Gender { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Cccd { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public DateTime DateOfBirth { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string DoctorDetailRecordId { get; set; } = string.Empty;

        public string DoctorImage { get; set; } = string.Empty;
        public string DoctorDegree { get; set; } = string.Empty;
        public string WorkingAtBranch { get; set; } = string.Empty;
        public string WorkingInDepartment { get; set; } = string.Empty;
        public string SpecializingIn { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string BranchIdRef { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string DepartmentIdRef { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string SpecialtyIdRef { get; set; } = string.Empty;
    }
    public class SearchDoctorCriteriaDto
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string BranchId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string DepartmentId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string SpecialtyId { get; set; }
    }


}