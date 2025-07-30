using MongoDB.Driver;
using System.Threading.Tasks;
using UserEntity = backend.Models.Entities.User;
using backend.Models.DTOs;
using Microsoft.Extensions.Options; // If you were to use IOptions<MongoDbSettings>
// No need for IOptions if directly injecting IMongoCollection

namespace backend.Services.GoogleAuth
{
    public class UserGGService
    {
        private readonly IMongoCollection<UserEntity> _users;

        // Modify constructor to take IMongoCollection<UserEntity>
        public UserGGService(IMongoCollection<UserEntity> users)
        {
            _users = users;
        }

        public async Task<UserEntity?> GetById(string id) =>
            await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

        public async Task<UserEntity?> GetByEmail(string email) =>
            await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

        public async Task<UserEntity?> GetByGoogleId(string googleId) =>
            await _users.Find(u => u.GoogleId == googleId).FirstOrDefaultAsync();

        public async Task CreateUser(UserEntity user) =>
            await _users.InsertOneAsync(user);

        public async Task UpdateUser(UserEntity user) =>
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);

        public async Task DeleteUser(string id) =>
            await _users.DeleteOneAsync(u => u.Id == id);
    }
}