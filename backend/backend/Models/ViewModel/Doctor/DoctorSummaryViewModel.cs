// backend/Models/ViewModel/Doctor/DoctorSummaryViewModel.cs
namespace backend.Models.ViewModel.Doctor
{
    public class DoctorSummaryViewModel
    {
        public string DoctorId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? DepartmentName { get; set; }
        public string? SpecialtyName { get; set; }
        public string? Degree { get; set; }
        // Có thể thêm các thuộc tính khác nếu cần cho Index view
    }
}