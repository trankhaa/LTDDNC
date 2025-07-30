// Trong backend.Models.DTOs (hoặc một namespace phù hợp)
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using backend.Models.Entities.Doctor; // Cho DoctorGender
using System;
using System.Collections.Generic;

namespace backend.Models.DTOs.Doctor
{
    public class CreateFullDoctorDto
    {
        // === Doctor Properties ===
        [Required(ErrorMessage = "Tên bác sĩ không được để trống")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giới tính không được để trống")]
        public DoctorGender Gender { get; set; }

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "CCCD/CMND không được để trống")]
        public string Cccd { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        // === DoctorDetail Properties ===
        [Required(ErrorMessage = "Bằng cấp không được để trống")]
        public string Degree { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "ID Chi nhánh không được để trống")]
        public string BranchId { get; set; } = string.Empty; // Sẽ là ObjectId string

        [Required(ErrorMessage = "ID Khoa không được để trống")]
        public string DepartmentId { get; set; } = string.Empty; // Sẽ là ObjectId string

        public string? SpecialtyId { get; set; } // Sẽ là ObjectId string, có thể null

        public IFormFile? ImgFile { get; set; } // Ảnh đại diện bác sĩ
        public IFormFile? CertificateImgFile { get; set; } // Ảnh chứng chỉ
        public IFormFile? DegreeImgFile { get; set; } // Ảnh bằng cấp

        // === DoctorSchedule Properties (Optional, for initial schedule) ===
        public int? ConsultationFee { get; set; }
        public string? StartTime { get; set; } // Format "HH:mm"
        public string? EndTime { get; set; }   // Format "HH:mm"
        public int? ExaminationTime { get; set; } // minutes
    }

    public enum DoctorGender
    {
        Male,
        Female,
        Other
    }
}