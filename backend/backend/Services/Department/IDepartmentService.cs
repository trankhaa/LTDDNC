using backend.Models.Entities;
using backend.Models.ViewModel;

namespace backend.Services
{
    public interface IDepartmentService
    {
        Task<List<Department>> GetAllDepartments();
        Task<Department?> GetDepartmentById(string id);
        Task<Department> CreateDepartment(DepartmentViewModel departmentVM);
        Task UpdateDepartment(string id, DepartmentViewModel departmentVM);
        Task DeleteDepartment(string id);
        Task<List<Specialty>> GetSpecialtiesByDepartment(string departmentId);
        Task<int> CountDepartmentsAsync();
    }
}