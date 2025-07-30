using backend.Models.Entities; // Thêm using này nếu class Branch nằm trong namespace này
using backend.Models.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface IBranchService
    {
        Task<List<backend.Models.Entities.Branch>> GetAllBranches(); // Fully qualify 'Branch' to resolve ambiguity
        Task<backend.Models.Entities.Branch?> GetBranchById(string id); // Fully qualify 'Branch' to resolve ambiguity
        Task<backend.Models.Entities.Branch> CreateBranch(BranchViewModel branchVM); // Fully qualify 'Branch' to resolve ambiguity
        Task UpdateBranch(string id, BranchViewModel branchVM);
        Task DeleteBranch(string id); Task<int> CountBranchesAsync();
    }
}