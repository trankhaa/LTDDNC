namespace backend.Models.DTOs
{
    public class SpecialtyDTOs
    {
        public string IdSpecialty { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DepartmentId { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; } // ✅ Thêm dòng này
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}