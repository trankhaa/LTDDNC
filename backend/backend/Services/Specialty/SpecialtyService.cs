using backend.Models.Entities;
using backend.Models.ViewModel;
using backend.Services.UploadFile;
using backend.ViewModels;
using MongoDB.Driver;

namespace backend.Services
{
    public class SpecialtyService : ISpecialtyService
    {
        private readonly IMongoCollection<Specialty> _specialties;
        private readonly IMongoCollection<Department> _departments;
        private readonly IUploadFileService _uploadFileService;
        private const string SpecialtyImageFolder = "specialty-images";

        public SpecialtyService(IMongoDatabase database, IUploadFileService uploadFileService)
        {
            _specialties = database.GetCollection<Specialty>("Specialties");
            _departments = database.GetCollection<Department>("Departments");
            _uploadFileService = uploadFileService;
        }

        public async Task<List<Specialty>> GetAllSpecialties()
        {
            return await _specialties.Find(_ => true).ToListAsync();
        }

        public async Task<Specialty?> GetSpecialtyById(string id)
        {
            return await _specialties.Find(s => s.IdSpecialty == id).FirstOrDefaultAsync();
        }

        public async Task<Specialty> CreateSpecialty(SpecialtyViewModel specialtyVM)
        {
            var department = await _departments.Find(d => d.IdDepartment == specialtyVM.DepartmentId).FirstOrDefaultAsync();
            if (department == null) throw new Exception("Department not found");

            string imageUrl = string.Empty;

            if (specialtyVM.ImageFile != null)
            {
                imageUrl = await _uploadFileService.UploadFileAsync(specialtyVM.ImageFile, SpecialtyImageFolder);
            }

            var specialty = new Specialty
            {
                SpecialtyName = specialtyVM.SpecialtyName,
                Description = specialtyVM.Description,
                Department = new DepartmentRef
                {
                    IdDepartment = department.IdDepartment,
                    DepartmentName = department.DepartmentName
                },
                ImageUrl = imageUrl
            };

            await _specialties.InsertOneAsync(specialty);
            return specialty;
        }

        public async Task UpdateSpecialty(string id, SpecialtyViewModel specialtyVM)
        {
            var specialty = await GetSpecialtyById(id);
            if (specialty == null) return;

            var department = await _departments.Find(d => d.IdDepartment == specialtyVM.DepartmentId).FirstOrDefaultAsync();
            if (department == null) throw new Exception("Department not found");

            // Delete old image if new one is uploaded
            if (specialtyVM.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(specialty.ImageUrl))
                {
                    _uploadFileService.DeleteFile(specialty.ImageUrl);
                }
                specialty.ImageUrl = await _uploadFileService.UploadFileAsync(specialtyVM.ImageFile, SpecialtyImageFolder);
            }

            specialty.SpecialtyName = specialtyVM.SpecialtyName;
            specialty.Description = specialtyVM.Description;
            specialty.Department = new DepartmentRef
            {
                IdDepartment = department.IdDepartment,
                DepartmentName = department.DepartmentName
            };
            specialty.UpdatedAt = DateTime.UtcNow;

            await _specialties.ReplaceOneAsync(s => s.IdSpecialty == id, specialty);
        }

        public async Task DeleteSpecialty(string id)
        {
            var specialty = await GetSpecialtyById(id);
            if (specialty == null) return;

            // Delete associated image
            if (!string.IsNullOrEmpty(specialty.ImageUrl))
            {
                _uploadFileService.DeleteFile(specialty.ImageUrl);
            }

            await _specialties.DeleteOneAsync(s => s.IdSpecialty == id);
        }
        public async Task<List<Specialty>> GetByDepartmentIdAsync(string departmentId)
        {
            return await _specialties.Find(s => s.Department.IdDepartment == departmentId).ToListAsync();
        }

    }
}