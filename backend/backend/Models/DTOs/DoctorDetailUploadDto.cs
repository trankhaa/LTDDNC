using Microsoft.AspNetCore.Http;

namespace backend.Models.DTOs
{
    public class DoctorDetailUploadDto
    {
        public string DoctorId { get; set; } = string.Empty;

        public string Degree { get; set; } = string.Empty;

        public string BranchId { get; set; } = string.Empty;

        public string DepartmentId { get; set; } = string.Empty;

        public string SpecialtyId { get; set; } = string.Empty;

        public string? Description { get; set; }

        public IFormFile? ImgFile { get; set; }

        public IFormFile? CertificateImgFile { get; set; }

        public IFormFile? DegreeImgFile { get; set; }
    }
    public class DoctorLogin
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
