using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.ViewModels
{
    public class PatientViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ và tên")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [Display(Name = "Giới tính")]
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Mã bệnh nhân là bắt buộc")]
        [Display(Name = "Mã bệnh nhân")]
        public string? PatientCode { get; set; }

        [Display(Name = "Nhóm máu")]
        public string? BloodType { get; set; }

        [Display(Name = "Chiều cao (cm)")]
        [Range(0, 300, ErrorMessage = "Chiều cao không hợp lệ")]
        public double? Height { get; set; }

        [Display(Name = "Cân nặng (kg)")]
        [Range(0, 500, ErrorMessage = "Cân nặng không hợp lệ")]
        public double? Weight { get; set; }

        [Display(Name = "Số BHYT")]
        public string? InsuranceNumber { get; set; }

        [Display(Name = "Tiền sử bệnh")]
        public List<string> MedicalHistory { get; set; } = new List<string>();

        [Display(Name = "Dị ứng")]
        public List<string> Allergies { get; set; } = new List<string>();

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime UpdatedAt { get; set; }
    }

    public class PatientCreateViewModel : PatientViewModel
    {
        // Có thể thêm các trường đặc biệt cho tạo mới
    }

    public class PatientUpdateViewModel : PatientViewModel
    {
        // Có thể thêm các trường đặc biệt cho cập nhật
    }
}