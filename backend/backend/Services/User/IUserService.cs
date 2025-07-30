using backend.Models.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services.User
{
    public interface IUserService
    {
        Task<IEnumerable<backend.Models.Entities.User>> GetAllUsersAsync();
        Task<IEnumerable<backend.Models.Entities.User>> GetUsersByRoleAsync(string role);
        Task<backend.Models.Entities.User?> GetUserByIdAsync(string id);
        Task<backend.Models.Entities.User?> GetUserByEmailAsync(string email);
        Task<bool> IsEmailUniqueAsync(string email, string? currentUserId = null);
        Task CreateUserAsync(UserCreateEditViewModel model);
        Task<bool> UpdateUserAsync(UserCreateEditViewModel model);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> CheckPasswordAsync(string email, string password);
        Task<bool> ValidateUserAsync(string email, string password);
        Task<int> CountPatientsAsync();
        Task<int> CountUsersAsync();
    }
}