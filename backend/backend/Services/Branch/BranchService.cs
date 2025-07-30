// Services/BranchService.cs
using backend.Models.Entities;
using backend.Models.ViewModel;
using backend.Services.UploadFile;
using MongoDB.Driver;

namespace backend.Services
{
    public class BranchService : IBranchService
    {
        private readonly IMongoCollection<Branch> _branches;
        private readonly IUploadFileService _uploadFileService;
        private const string BranchImageFolder = "branch-images";

        public BranchService(IMongoDatabase database, IUploadFileService uploadFileService)
        {
            _branches = database.GetCollection<Branch>("Branches");
            _uploadFileService = uploadFileService;
        }
        public async Task<int> CountBranchesAsync()
        {
            return (int)await _branches.CountDocumentsAsync(_ => true);
        }

        public async Task<List<Branch>> GetAllBranches()
        {
            return await _branches.Find(_ => true).ToListAsync();
        }

        public async Task<Branch?> GetBranchById(string id)
        {
            return await _branches.Find(b => b.IdBranch == id).FirstOrDefaultAsync();
        }

        public async Task<Branch> CreateBranch(BranchViewModel branchVM)
        {
            string imageUrl = string.Empty;

            if (branchVM.ImageFile != null)
            {
                imageUrl = await _uploadFileService.UploadFileAsync(branchVM.ImageFile, BranchImageFolder);
            }

            GeoPoint? coordinates = null;
            if (branchVM.Latitude.HasValue && branchVM.Longitude.HasValue)
            {
                coordinates = new GeoPoint
                {
                    Latitude = branchVM.Latitude.Value,
                    Longitude = branchVM.Longitude.Value
                };
            }

            var branch = new Branch
            {
                BranchName = branchVM.BranchName,
                BranchAddress = branchVM.BranchAddress,
                BranchHotline = branchVM.BranchHotline,
                BranchEmail = branchVM.BranchEmail,
                Description = branchVM.Description,
                Coordinates = coordinates,
                ImageUrl = imageUrl
            };

            await _branches.InsertOneAsync(branch);
            return branch;
        }

        public async Task UpdateBranch(string id, BranchViewModel branchVM)
        {
            var branch = await GetBranchById(id);
            if (branch == null) return;

            // Delete old image if new one is uploaded
            if (branchVM.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(branch.ImageUrl))
                {
                    _uploadFileService.DeleteFile(branch.ImageUrl);
                }
                branch.ImageUrl = await _uploadFileService.UploadFileAsync(branchVM.ImageFile, BranchImageFolder);
            }

            branch.BranchName = branchVM.BranchName;
            branch.BranchAddress = branchVM.BranchAddress;
            branch.BranchHotline = branchVM.BranchHotline;
            branch.BranchEmail = branchVM.BranchEmail;
            branch.Description = branchVM.Description;

            if (branchVM.Latitude.HasValue && branchVM.Longitude.HasValue)
            {
                branch.Coordinates ??= new GeoPoint();
                branch.Coordinates.Latitude = branchVM.Latitude.Value;
                branch.Coordinates.Longitude = branchVM.Longitude.Value;
            }
            else
            {
                branch.Coordinates = null;
            }

            branch.UpdatedAt = DateTime.UtcNow;

            await _branches.ReplaceOneAsync(b => b.IdBranch == id, branch);
        }

        public async Task DeleteBranch(string id)
        {
            var branch = await GetBranchById(id);
            if (branch == null) return;

            // Delete associated image
            if (!string.IsNullOrEmpty(branch.ImageUrl))
            {
                _uploadFileService.DeleteFile(branch.ImageUrl);
            }

            await _branches.DeleteOneAsync(b => b.IdBranch == id);
        }
    }
}