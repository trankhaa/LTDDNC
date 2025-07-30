using Microsoft.AspNetCore.Http;
using backend.Models.Entities.Doctor;

namespace backend.Models.DTOs;

public class DoctorFullInfoDto
{
    public global::backend.Models.Entities.Doctor.Doctor Doctor { get; set; }
    public DoctorDetail DoctorDetail { get; set; }
    public List<DoctorSchedule> DoctorSchedules { get; set; } = new();
    public string? BranchName { get; set; }
    public string? DepartmentName { get; set; }
    public string? SpecialtyName { get; set; }
}
