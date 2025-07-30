// Services/Packages/IPackageService.cs
using backend.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.Entities;

namespace backend.Services.Packages
{
    public interface IPackageService
    {
        Task<List<Package>> GetAllAsync();
        Task<List<Package>> GetActiveAsync();
        Task<Package?> GetByIdAsync(string id);
        Task CreateAsync(Package newPackage);
        Task UpdateAsync(string id, Package updatedPackage);
        Task DeleteAsync(string id);
    }
}