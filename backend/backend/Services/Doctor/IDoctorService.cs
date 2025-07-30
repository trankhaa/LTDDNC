using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.Entities.Doctor;
using backend.Models.DTOs.Doctor; // << Namespace cho CreateFullDoctorDto

namespace backend.Services
{
    public interface IDoctorService
    {
        Task<List<Doctor>> GetAllAsync();

        Task AddAsync(Doctor newDoctor);

        Task RegisterDoctorAsync(Doctor newDoctor);

        Task<bool> IsDuplicateAsync(string email, string phone);
        Task<(bool Success, string? ErrorMessage)> UpdateDoctorWithDetailsAsync(string doctorId, UpdateFullDoctorDto dto);

        Task<Doctor?> GetByEmailAsync(string email);
        Task<Doctor?> GetByIdAsync(string id);

        bool VerifyPassword(Doctor doctor, string providedPassword);

        Task<(bool Success, string? ErrorMessage, Doctor? CreatedDoctor)> CreateDoctorWithDetailsAsync(CreateFullDoctorDto dto); // << Sử dụng CreateFullDoctorDto
        Task<int> CountDoctorsAsync(); Task<(bool Success, string? ErrorMessage)> DeleteDoctorAsync(string doctorId);
    }
}