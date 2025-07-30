using backend.Models.Entities;
using backend.Models.Entities.Doctor;
using backend.Models.Entities.Booking;
using backend.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace backend.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDbSettings _settings;

        public MongoDbContext(IOptions<MongoDbSettings> options)
        {
            _settings = options.Value;
            var client = new MongoClient(_settings.ConnectionString);
            _database = client.GetDatabase(_settings.DatabaseName);
        }

        // User related collections
        public IMongoCollection<User> Users => _database.GetCollection<User>(_settings.Collections.Users);
        // public IMongoCollection<UserQuestion> UserQuestions => _database.GetCollection<UserQuestion>(_settings.Collections.UserQuestions);

        // Doctor related collections
        public IMongoCollection<Doctor> Doctors => _database.GetCollection<Doctor>(_settings.Collections.Doctors);
        public IMongoCollection<DoctorDetail> DoctorDetails => _database.GetCollection<DoctorDetail>(_settings.Collections.DoctorDetails);
        // public IMongoCollection<DoctorReview> DoctorReviews => _database.GetCollection<DoctorReview>(_settings.Collections.DoctorReviews);
        public IMongoCollection<DoctorSchedule> DoctorSchedules => _database.GetCollection<DoctorSchedule>(_settings.Collections.DoctorSchedules);

        // Clinic management collections
        public IMongoCollection<Branch> Branches => _database.GetCollection<Branch>(_settings.Collections.Branches);
        public IMongoCollection<Department> Departments => _database.GetCollection<Department>(_settings.Collections.Departments);
        public IMongoCollection<Specialty> Specialties => _database.GetCollection<Specialty>(_settings.Collections.Specialties);

        // Patient related collections
        public IMongoCollection<Patient> Patients => _database.GetCollection<Patient>(_settings.Collections.Patients);
        // public IMongoCollection<MedicalRecord> MedicalRecords => _database.GetCollection<MedicalRecord>(_settings.Collections.MedicalRecords);

        // Booking & payment collections
        public IMongoCollection<ConfirmAppointment> ConfirmAppointment => _database.GetCollection<ConfirmAppointment>(_settings.Collections.ConfirmAppointment);
        // public IMongoCollection<Payment> Payments => _database.GetCollection<Payment>(_settings.Collections.Payments);

        // Other collections
        // public IMongoCollection<AdminAnswer> AdminAnswers => _database.GetCollection<AdminAnswer>(_settings.Collections.AdminAnswers);
        // public IMongoCollection<AI_Chat_Log> ALChatLogs => _database.GetCollection<AI_Chat_Log>(_settings.Collections.ALChatLogs);
        // public IMongoCollection<Packages> Packages => _database.GetCollection<Packages>(_settings.Collections.Packages);
   
    }
}
