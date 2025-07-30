// File: backend/Models/ViewModel/Doctor/DoctorFullInfoViewModel.cs
using System;

namespace backend.Models.ViewModel.Doctor
{
    public class DoctorFullInfoViewModel
    {
        // === Thông tin từ Entity Doctor ===
        public string? DoctorId { get; set; }
        public string? Name { get; set; }
        public string? Gender { get; set; } // Sẽ là string ("Nam", "Nữ", "Khác") sau khi chuyển từ enum DoctorGender
        public DateTime? DateOfBirth { get; set; } // Nên là DateTime? để xử lý trường hợp không có dữ liệu
        public string? Cccd { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; } // Giả sử luôn có khi Doctor được tạo
        public DateTime UpdatedAt { get; set; } // Giả sử luôn có khi Doctor được tạo

        // === Thông tin từ Entity DoctorDetail ===
        public string? AvatarUrl { get; set; } // Đường dẫn đến ảnh đại diện
        public string? CertificateImgUrl { get; set; } // Đường dẫn đến ảnh chứng chỉ
        public string? DegreeImgUrl { get; set; } // Đường dẫn đến ảnh bằng cấp
        public string? Degree { get; set; }
        public string? Description { get; set; }
        public string? BranchName { get; set; } // Sẽ lấy từ DoctorDetail.BranchName
        public string? DepartmentName { get; set; } // Sẽ lấy từ DoctorDetail.DepartmentName
        public string? SpecialtyName { get; set; } // Sẽ lấy từ DoctorDetail.SpecialtyName

        // === Thông tin từ Entity DoctorSchedule (nếu có) ===
        public decimal? ConsultationFee { get; set; } // Sử dụng decimal? để cho phép null
        public string? StartTime { get; set; } // Format "HH:mm"
        public string? EndTime { get; set; }   // Format "HH:mm"
        public int? ExaminationTime { get; set; } // Số phút, int? để cho phép null

        // Constructor (tùy chọn, có thể không cần thiết nếu bạn map thủ công trong controller)
        public DoctorFullInfoViewModel()
        {
            // Có thể khởi tạo giá trị mặc định nếu muốn
            // Ví dụ: Name = "N/A";
        }
    }
}