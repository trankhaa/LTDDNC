// Services/Packages/PackageService.cs
using backend.Models.Entities;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services.Packages
{
    public class PackageService : IPackageService
    {
        private readonly IMongoCollection<Package> _packagesCollection;

        public PackageService(IMongoDatabase database)
        {
            _packagesCollection = database.GetCollection<Package>("Packages");
        }

        public async Task<List<Package>> GetAllAsync() =>
            await _packagesCollection.Find(_ => true).SortByDescending(p => p.CreatedAt).ToListAsync();

        public async Task<List<Package>> GetActiveAsync() =>
            await _packagesCollection.Find(p => p.IsActive).SortByDescending(p => p.CreatedAt).ToListAsync();

        public async Task<Package?> GetByIdAsync(string id) =>
            await _packagesCollection.Find(p => p.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Package newPackage) =>
            await _packagesCollection.InsertOneAsync(newPackage);

        public async Task UpdateAsync(string id, Package updatedPackage) =>
            await _packagesCollection.ReplaceOneAsync(p => p.Id == id, updatedPackage);

        public async Task DeleteAsync(string id) =>
            await _packagesCollection.DeleteOneAsync(p => p.Id == id);
    }
}