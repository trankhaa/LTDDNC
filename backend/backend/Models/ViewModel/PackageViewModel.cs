using System.ComponentModel.DataAnnotations;

namespace backend.Models.ViewModel
{
    public class PackageViewModel
    {
        public string? IdPackage { get; set; }

        [Required(ErrorMessage = "Package name is required")]
        [StringLength(100, ErrorMessage = "Package name cannot exceed 100 characters")]
        public string PackageName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100,000")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Specialty is required")]
        public string IdSpecialty { get; set; } = string.Empty;

        public string? SpecialtyName { get; set; }
    }
}
