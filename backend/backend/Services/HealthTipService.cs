// File: Services/HealthTipService.cs
using backend.Models.Entities;
using backend.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.Entities;

namespace backend.Services
{
    public class HealthTipService
    {
        private readonly IMongoCollection<HealthTip> _healthTipCollection;

        public HealthTipService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _healthTipCollection = mongoDatabase.GetCollection<HealthTip>(mongoDbSettings.Value.HealthTipCollectionName);
        }

        // Lấy tất cả mẹo, có thể lọc theo category
        public async Task<List<HealthTip>> GetAllAsync(string? category = null)
        {
            var filter = Builders<HealthTip>.Filter.Empty;
            if (!string.IsNullOrEmpty(category))
            {
                filter = Builders<HealthTip>.Filter.Eq(tip => tip.Category, category);
            }
            return await _healthTipCollection.Find(filter).SortByDescending(tip => tip.CreatedAt).ToListAsync();
        }

        // Lấy một mẹo theo ID (cho trang chi tiết)
        public async Task<HealthTip?> GetByIdAsync(string id) =>
            await _healthTipCollection.Find(tip => tip.Id == id).FirstOrDefaultAsync();

        // Lấy các mẹo nổi bật để hiển thị ở trang chủ
        public async Task<List<HealthTip>> GetFeaturedTipsAsync(int limit = 3)
        {
            var filter = Builders<HealthTip>.Filter.Eq(tip => tip.IsFeatured, true);
            return await _healthTipCollection.Find(filter)
                                             .SortByDescending(tip => tip.CreatedAt)
                                             .Limit(limit)
                                             .ToListAsync();
        }

        // (Tùy chọn) Hàm để tạo mới một mẹo
        public async Task CreateAsync(HealthTip newTip) =>
            await _healthTipCollection.InsertOneAsync(newTip);

        // (Tùy chọn) Hàm để cập nhật
        public async Task<bool> UpdateAsync(string id, HealthTip updatedTip)
        {
            var result = await _healthTipCollection.ReplaceOneAsync(tip => tip.Id == id, updatedTip);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        // (Tùy chọn) Hàm để xóa
        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _healthTipCollection.DeleteOneAsync(tip => tip.Id == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}