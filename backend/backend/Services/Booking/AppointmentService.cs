using backend.Models.Entities.Booking;
using backend.ViewModels.Booking;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services
{
    public interface IAppointmentService
    {
        Task<List<ConfirmAppointment>> GetAllAsync();
        Task<ConfirmAppointment> GetByIdAsync(string id);
        Task CreateAsync(ConfirmAppointment appointment);
        Task UpdateAsync(string id, ConfirmAppointment appointment);
        Task DeleteAsync(string id);
        Task<List<ConfirmAppointment>> SearchAsync(string searchTerm);
        Task<List<ConfirmAppointment>> FilterByDoctorAsync(string doctorId);
        Task<List<ConfirmAppointment>> FilterByDateAsync(DateTime date);
    }

    public class AppointmentService : IAppointmentService
    {
        private readonly IMongoCollection<ConfirmAppointment> _appointments;

        public AppointmentService(IMongoDatabase database)
        {
            _appointments = database.GetCollection<ConfirmAppointment>("ConfirmAppointments");
        }

        public async Task<List<ConfirmAppointment>> GetAllAsync()
        {
            return await _appointments.Find(_ => true).ToListAsync();
        }

        public async Task<ConfirmAppointment> GetByIdAsync(string id)
        {
            return await _appointments.Find(a => a.IdConfirmAppointment == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(ConfirmAppointment appointment)
        {
            await _appointments.InsertOneAsync(appointment);
        }

        public async Task UpdateAsync(string id, ConfirmAppointment appointment)
        {
            appointment.UpdatedAt = DateTime.UtcNow;
            await _appointments.ReplaceOneAsync(a => a.IdConfirmAppointment == id, appointment);
        }

        public async Task DeleteAsync(string id)
        {
            await _appointments.DeleteOneAsync(a => a.IdConfirmAppointment == id);
        }

        public async Task<List<ConfirmAppointment>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return await GetAllAsync();

            var filter = Builders<ConfirmAppointment>.Filter.Or(
                Builders<ConfirmAppointment>.Filter.Regex(a => a.NameDr, new BsonRegularExpression(searchTerm, "i")),
                Builders<ConfirmAppointment>.Filter.Regex(a => a.PatientEmail, new BsonRegularExpression(searchTerm, "i")),
                Builders<ConfirmAppointment>.Filter.Regex(a => a.Symptoms, new BsonRegularExpression(searchTerm, "i"))
            );

            return await _appointments.Find(filter).ToListAsync();
        }

        public async Task<List<ConfirmAppointment>> FilterByDoctorAsync(string doctorId)
        {
            if (string.IsNullOrEmpty(doctorId))
                return await GetAllAsync();

            return await _appointments.Find(a => a.DoctorId == doctorId).ToListAsync();
        }

        public async Task<List<ConfirmAppointment>> FilterByDateAsync(DateTime date)
        {
            return await _appointments.Find(a => a.Date.Date == date.Date).ToListAsync();
        }
    }
}