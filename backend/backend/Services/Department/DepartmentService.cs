// Services/DepartmentService.cs
using backend.Models.Entities;
using backend.Models.ViewModel;
using backend.Services.UploadFile;
using backend.ViewModels;
using MongoDB.Driver;

namespace backend.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IMongoCollection<Department> _departments;
        private readonly IMongoCollection<Specialty> _specialties;
        private readonly IUploadFileService _uploadFileService;
        private const string DepartmentImageFolder = "department-images";

        public DepartmentService(IMongoDatabase database, IUploadFileService uploadFileService)
        {
            _departments = database.GetCollection<Department>("Departments");
            _specialties = database.GetCollection<Specialty>("Specialties");
            _uploadFileService = uploadFileService;
        }

        public async Task<List<Department>> GetAllDepartments()
        {
            return await _departments.Find(_ => true).ToListAsync();
        }

        public async Task<Department?> GetDepartmentById(string id)
        {
            return await _departments.Find(d => d.IdDepartment == id).FirstOrDefaultAsync();
        }

        public async Task<Department> CreateDepartment(DepartmentViewModel departmentVM)
        {
            string imageUrl = string.Empty;

            if (departmentVM.ImageFile != null)
            {
                imageUrl = await _uploadFileService.UploadFileAsync(departmentVM.ImageFile, DepartmentImageFolder);
            }

            var department = new Department
            {
                DepartmentName = departmentVM.DepartmentName,
                Description = departmentVM.Description,
                ImageUrl = imageUrl
            };

            await _departments.InsertOneAsync(department);
            return department;
        }

        public async Task UpdateDepartment(string id, DepartmentViewModel departmentVM)
        {
            var department = await GetDepartmentById(id);
            if (department == null) return;

            // Delete old image if new one is uploaded
            if (departmentVM.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(department.ImageUrl))
                {
                    _uploadFileService.DeleteFile(department.ImageUrl);
                }
                department.ImageUrl = await _uploadFileService.UploadFileAsync(departmentVM.ImageFile, DepartmentImageFolder);
            }

            department.DepartmentName = departmentVM.DepartmentName;
            department.Description = departmentVM.Description;
            department.UpdatedAt = DateTime.UtcNow;

            await _departments.ReplaceOneAsync(d => d.IdDepartment == id, department);
        }

        public async Task DeleteDepartment(string id)
        {
            var department = await GetDepartmentById(id);
            if (department == null) return;

            // Delete associated image
            if (!string.IsNullOrEmpty(department.ImageUrl))
            {
                _uploadFileService.DeleteFile(department.ImageUrl);
            }

            await _departments.DeleteOneAsync(d => d.IdDepartment == id);
        }

        public async Task<List<Specialty>> GetSpecialtiesByDepartment(string departmentId)
        {
            return await _specialties.Find(s => s.Department.IdDepartment == departmentId).ToListAsync();
        }
        
        public async Task<int> CountDepartmentsAsync()
        {
            return (int)await _departments.CountDocumentsAsync(_ => true);
        }
    }
}