// File: backend/Services/Booking/ConfirmAppointmentService.cs
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using backend.Models.Entities.Booking;
using backend.Settings;
using backend.Models.Entities.Doctor;
using backend.Models.Entities;
using Microsoft.AspNetCore.Hosting; // Thêm using này nếu bạn dùng _env
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    public class ConfirmAppointmentService
    {
        private readonly IMongoCollection<ConfirmAppointment> _confirmAppointmentCollection;
        private readonly IMongoCollection<Doctor> _doctorCollection;
        // ✅ Code đã sửa
        private readonly IMongoCollection<backend.Models.Entities.User> _userCollection;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;

        public ConfirmAppointmentService(IOptions<MongoDbSettings> mongoDbSettings, IWebHostEnvironment env, IEmailService emailService)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);

            // ✅ Code đã sửa
            _userCollection = mongoDatabase.GetCollection<backend.Models.Entities.User>(mongoDbSettings.Value.UserCollectionName);
            _confirmAppointmentCollection = mongoDatabase.GetCollection<ConfirmAppointment>(mongoDbSettings.Value.ConfirmAppointmentCollectionName);
            _doctorCollection = mongoDatabase.GetCollection<Doctor>(mongoDbSettings.Value.DoctorCollectionName);
            _emailService = emailService;
            _env = env;
        }

        public async Task CreateAppointmentAsync(ConfirmAppointment appointment)
        {
            appointment.CreatedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _confirmAppointmentCollection.InsertOneAsync(appointment);

            // Gửi email xác nhận với tham số đã sửa đúng
            await _emailService.SendBookingConfirmationEmailAsync(
                toEmail: appointment.PatientEmail,
                patientName: appointment.PatientName,
                doctorName: appointment.NameDr,
                appointmentTime: appointment.Date,
                location: appointment.Slot, // Giả sử Slot là địa điểm/phòng khám
                note: appointment.Symptoms,
                price: appointment.ConsultationFee
            );
        }

        public async Task<bool> IsSlotTakenAsync(string doctorId, DateTime date, string slot)
        {
            var filter = Builders<ConfirmAppointment>.Filter.Eq(a => a.DoctorId, doctorId) &
                         Builders<ConfirmAppointment>.Filter.Eq(a => a.Date, date.Date) &
                         Builders<ConfirmAppointment>.Filter.Eq(a => a.Slot, slot);
            return await _confirmAppointmentCollection.Find(filter).AnyAsync();
        }

        public async Task<List<ConfirmAppointment>> GetAppointmentsByDoctorIdAsync(string doctorId)
        {
            var filter = Builders<ConfirmAppointment>.Filter.Eq(a => a.DoctorId, doctorId);
            return await _confirmAppointmentCollection.Find(filter).ToListAsync();
        }

        public async Task<List<ConfirmAppointment>> GetAppointmentsByDoctorAndDateAsync(string doctorId, DateTime date)
        {
            var filter = Builders<ConfirmAppointment>.Filter.Eq(a => a.DoctorId, doctorId) &
                         Builders<ConfirmAppointment>.Filter.Eq(a => a.Date, date.Date);
            return await _confirmAppointmentCollection.Find(filter).ToListAsync();
        }

        public async Task<bool> CheckPatientBookingAsync(string patientId, string doctorId, DateTime date, string slot)
        {
            var filter = Builders<ConfirmAppointment>.Filter.Eq(a => a.PatientId, patientId) &
                         Builders<ConfirmAppointment>.Filter.Eq(a => a.DoctorId, doctorId) &
                         Builders<ConfirmAppointment>.Filter.Eq(a => a.Date, date.Date) &
                         Builders<ConfirmAppointment>.Filter.Eq(a => a.Slot, slot);

            return await _confirmAppointmentCollection.Find(filter).AnyAsync();
        }

        public async Task<bool> HasPatientBookedWithDoctorOnDateAsync(string patientId, string doctorId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var filter = Builders<ConfirmAppointment>.Filter.Eq(a => a.PatientId, patientId) &
                         Builders<ConfirmAppointment>.Filter.Eq(a => a.DoctorId, doctorId) &
                         Builders<ConfirmAppointment>.Filter.Gte(a => a.Date, startOfDay) &
                         Builders<ConfirmAppointment>.Filter.Lte(a => a.Date, endOfDay);

            return await _confirmAppointmentCollection.Find(filter).AnyAsync();
        }

        public async Task<bool> UpdateAppointmentStatusByOrderCodeAsync(long orderCode, string newStatus)
        {
            var filter = Builders<ConfirmAppointment>.Filter.Eq(a => a.OrderCode, orderCode);
            var existingAppointment = await _confirmAppointmentCollection.Find(filter).FirstOrDefaultAsync();

            if (existingAppointment == null)
            {
                Console.WriteLine($"[INFO] Không tìm thấy lịch hẹn với OrderCode: {orderCode}");
                return false;
            }

            if (existingAppointment.Status == newStatus)
            {
                Console.WriteLine($"[INFO] Lịch hẹn {orderCode} đã ở trạng thái {newStatus}.");
                return false;
            }

            var update = Builders<ConfirmAppointment>.Update
                .Set(a => a.Status, newStatus)
                .Set(a => a.UpdatedAt, DateTime.UtcNow);

            var result = await _confirmAppointmentCollection.UpdateOneAsync(filter, update);

            if (result.IsAcknowledged && result.ModifiedCount > 0)
            {
                if (newStatus == PaymentStatus.PAID)
                {
                    try
                    {
                        // Gửi email xác nhận với tham số đã sửa đúng
                        await _emailService.SendBookingConfirmationEmailAsync(
                            toEmail: existingAppointment.PatientEmail,
                            patientName: existingAppointment.PatientName,
                            doctorName: existingAppointment.NameDr,
                            appointmentTime: existingAppointment.Date,
                            location: existingAppointment.Slot,
                            note: existingAppointment.Symptoms,
                            price: existingAppointment.ConsultationFee
                        );
                        Console.WriteLine($"[INFO] Đã gửi email xác nhận thanh toán cho OrderCode: {orderCode}");
                    }
                    catch (Exception emailEx)
                    {
                        Console.WriteLine($"[WARNING] Gửi email xác nhận thất bại cho OrderCode {orderCode}. Lỗi: {emailEx.Message}");
                    }
                }
                return true;
            }

            return false;
        }
    }
}