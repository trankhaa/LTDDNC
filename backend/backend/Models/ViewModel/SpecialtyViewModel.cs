using System.ComponentModel.DataAnnotations;

namespace backend.Models.ViewModel
{
    public class SpecialtyViewModel
    {
        public string? IdSpecialty { get; set; }

        [Required(ErrorMessage = "Specialty name is required")]
        public string SpecialtyName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        public string DepartmentId { get; set; } = string.Empty;

        public string? DepartmentName { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? ImageFile { get; set; }

        public string? ImageUrl { get; set; }
    }
}
