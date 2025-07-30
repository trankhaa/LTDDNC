using backend.Models.ViewModel;
using backend.Services.UploadFile;
using backend.Helper;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services.User
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<backend.Models.Entities.User> _users;
        private readonly IUploadFileService _uploadFileService;
        private readonly IBcryptHelper _bcryptHelper;

        public UserService(
            IMongoCollection<backend.Models.Entities.User> users,
            IUploadFileService uploadFileService,
            IBcryptHelper bcryptHelper)
        {
            _users = users;
            _uploadFileService = uploadFileService;
            _bcryptHelper = bcryptHelper;
        }

        public async Task<IEnumerable<backend.Models.Entities.User>> GetAllUsersAsync()
        {
            var projection = Builders<backend.Models.Entities.User>.Projection
                .Exclude("Name"); // Loại bỏ trường Name nếu có trong database

            return await _users.Find(user => true)
                .Project<backend.Models.Entities.User>(projection)
                .SortByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<backend.Models.Entities.User>> GetUsersByRoleAsync(string role)
        {
            return await _users.Find(user => user.Role == role)
                              .SortByDescending(u => u.CreatedAt)
                              .ToListAsync();
        }

        public async Task<backend.Models.Entities.User?> GetUserByIdAsync(string id)
        {
            return await _users.Find(user => user.Id == id)
                              .FirstOrDefaultAsync();
        }

        public async Task<backend.Models.Entities.User?> GetUserByEmailAsync(string email)
        {
            return await _users.Find(user => user.Email.ToLower() == email.ToLower())
                              .FirstOrDefaultAsync();
        }

        public async Task<bool> IsEmailUniqueAsync(string email, string? currentUserId = null)
        {
            var user = await GetUserByEmailAsync(email);
            return user == null || user.Id == currentUserId;
        }

        public async Task CreateUserAsync(UserCreateEditViewModel model)
        {
            var user = new backend.Models.Entities.User
            {
                Email = model.Email,
                Password = _bcryptHelper.HashPassword(model.Password ?? string.Empty),
                Role = model.Role,
                IsActive = true,
                DoctorId = model.Role == "Doctor" ? model.Id : null,
                PatientId = model.Role == "Patient" ? model.Id : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _users.InsertOneAsync(user);
        }

        public async Task<bool> UpdateUserAsync(UserCreateEditViewModel model)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                return false;
            }
            var user = await GetUserByIdAsync(model.Id);
            if (user == null) return false;

            // Update user properties
            user.Email = model.Email;
            user.Role = model.Role;
            user.UpdatedAt = DateTime.UtcNow;

            // Update password if provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.Password = _bcryptHelper.HashPassword(model.Password);
            }

            // Update doctor/patient IDs based on role
            if (model.Role == "Doctor")
            {
                user.DoctorId = model.Id;
                user.PatientId = null;
            }
            else if (model.Role == "Patient")
            {
                user.PatientId = model.Id;
                user.DoctorId = null;
            }
            else
            {
                user.DoctorId = null;
                user.PatientId = null;
            }

            var result = await _users.ReplaceOneAsync(
                u => u.Id == model.Id,
                user,
                new ReplaceOptions { IsUpsert = false });

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var result = await _users.DeleteOneAsync(u => u.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        public async Task<bool> CheckPasswordAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null) return false;
            return _bcryptHelper.VerifyPassword(password, user.Password);
        }

        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null) return false;

            // So sánh mật khẩu đã hash
            return _bcryptHelper.VerifyPassword(password, user.Password);
        }

        public async Task<int> CountPatientsAsync()
        {
            return (int)await _users.CountDocumentsAsync(u => u.Role == "Patient");
        }
        public async Task<int> CountUsersAsync()
        {
            return (int)await _users.CountDocumentsAsync(_ => true);
        }
    }
}