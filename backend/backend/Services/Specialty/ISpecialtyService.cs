// Services/ISpecialtyService.cs
using backend.Models.Entities;
using backend.Models.ViewModel;
using backend.ViewModels;

namespace backend.Services
{
    public interface ISpecialtyService
    {
        Task<List<Specialty>> GetAllSpecialties();
        Task<Specialty?> GetSpecialtyById(string id);
        Task<Specialty> CreateSpecialty(SpecialtyViewModel specialtyVM);
        Task UpdateSpecialty(string id, SpecialtyViewModel specialtyVM);
        Task DeleteSpecialty(string id);
        Task<List<Specialty>> GetByDepartmentIdAsync(string departmentId);

    }
}