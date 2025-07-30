using MongoDB.Driver;
using backend.Models.Entities.Doctor;
using Microsoft.Extensions.Options;
using backend.Data;
using backend.Settings;

namespace backend.Services

{
    public class DoctorScheduleService
    {
        private readonly IMongoCollection<DoctorSchedule> _doctorScheduleCollection;

        public DoctorScheduleService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var client = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _doctorScheduleCollection = database.GetCollection<DoctorSchedule>(mongoDbSettings.Value.DoctorScheduleCollectionName);
        }

        public async Task<List<DoctorSchedule>> GetAllAsync() =>
            await _doctorScheduleCollection.Find(_ => true).ToListAsync();

        public async Task<List<DoctorSchedule>> GetDoctorScheduleByDoctorIdAsync(string doctorId) =>
            await _doctorScheduleCollection.Find(s => s.DoctorId == doctorId).ToListAsync();

        public async Task<DoctorSchedule?> GetByIdAsync(string id) =>
            await _doctorScheduleCollection.Find(s => s.IdDoctorSchedule == id).FirstOrDefaultAsync();

        public async Task AddAsync(DoctorSchedule schedule) =>
            await _doctorScheduleCollection.InsertOneAsync(schedule);

        public async Task<bool> UpdateAsync(string id, DoctorSchedule updated)
        {
            var result = await _doctorScheduleCollection.ReplaceOneAsync(s => s.IdDoctorSchedule == id, updated);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _doctorScheduleCollection.DeleteOneAsync(s => s.IdDoctorSchedule == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }
    }
}