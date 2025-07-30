using System.ComponentModel.DataAnnotations;

namespace backend.Models.ViewModel
{
    public class DepartmentViewModel
    {
        public string? IdDepartment { get; set; }

        [Required(ErrorMessage = "Department name is required")]
        public string DepartmentName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [DataType(DataType.Upload)]
        public IFormFile? ImageFile { get; set; }

        public string? ImageUrl { get; set; }
    }
}
