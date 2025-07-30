using backend.Models.Entities;
using backend.Services.Patient;
using backend.ViewModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services
{
    public class PatientService : IPatientService
    {
        private readonly IMongoCollection<backend.Models.Entities.Patient> _patients;

        public PatientService(IMongoDatabase database)
        {
            _patients = database.GetCollection<backend.Models.Entities.Patient>("Patients");
        }

        public async Task<List<PatientViewModel>> GetAllPatientsAsync()
        {
            var patients = await _patients.Find(_ => true).ToListAsync();
            return patients.ConvertAll(p => MapToViewModel(p)!);
        }

        public async Task<PatientViewModel> GetPatientByIdAsync(string id)
        {
            Models.Entities.Patient patient = await _patients.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (patient == null)
                throw new KeyNotFoundException($"Patient with ID {id} not found");
            return MapToViewModel(patient)!;
        }

        public async Task<PatientViewModel> CreatePatientAsync(PatientCreateViewModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var patient = new backend.Models.Entities.Patient
            {
                FullName = model.FullName?.Trim() ?? throw new ArgumentException("FullName is required"),
                Gender = ParseGender(model.Gender),
                DateOfBirth = model.DateOfBirth,
                Phone = model.Phone?.Trim() ?? string.Empty,
                Address = model.Address?.Trim() ?? string.Empty,
                PatientCode = model.PatientCode?.Trim() ?? throw new ArgumentException("PatientCode is required"),
                BloodType = model.BloodType?.Trim(),
                Height = model.Height,
                Weight = model.Weight,
                InsuranceNumber = model.InsuranceNumber?.Trim(),
                MedicalHistory = model.MedicalHistory ?? new List<string>(),
                Allergies = model.Allergies ?? new List<string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _patients.InsertOneAsync(patient);
            return MapToViewModel(patient)!;
        }

        public async Task UpdatePatientAsync(string id, PatientUpdateViewModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("ID cannot be null or empty", nameof(id));

            var update = Builders<backend.Models.Entities.Patient>.Update
                .Set(p => p.FullName, model.FullName?.Trim() ?? throw new ArgumentException("FullName is required"))
                .Set(p => p.Gender, ParseGender(model.Gender))
                .Set(p => p.DateOfBirth, model.DateOfBirth)
                .Set(p => p.Phone, model.Phone?.Trim())
                .Set(p => p.Address, model.Address?.Trim())
                .Set(p => p.PatientCode, model.PatientCode?.Trim() ?? throw new ArgumentException("PatientCode is required"))
                .Set(p => p.BloodType, model.BloodType?.Trim())
                .Set(p => p.Height, model.Height)
                .Set(p => p.Weight, model.Weight)
                .Set(p => p.InsuranceNumber, model.InsuranceNumber?.Trim())
                .Set(p => p.MedicalHistory, model.MedicalHistory ?? new List<string>())
                .Set(p => p.Allergies, model.Allergies ?? new List<string>())
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            var result = await _patients.UpdateOneAsync(
                p => p.Id == id, 
                update);

            if (result.MatchedCount == 0)
                throw new KeyNotFoundException($"Patient with ID {id} not found");
        }

        public async Task DeletePatientAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) 
                throw new ArgumentException("ID cannot be null or empty", nameof(id));

            var result = await _patients.DeleteOneAsync(p => p.Id == id);

            if (result.DeletedCount == 0)
                throw new KeyNotFoundException($"Patient with ID {id} not found");
        }

        private static PatientViewModel? MapToViewModel(backend.Models.Entities.Patient? patient)
        {
            if (patient == null) return null;

            return new PatientViewModel
            {
                Id = patient.Id,
                FullName = patient.FullName,
                Gender = patient.Gender.ToString(),
                DateOfBirth = patient.DateOfBirth,
                Phone = patient.Phone,
                Address = patient.Address,
                PatientCode = patient.PatientCode,
                BloodType = patient.BloodType,
                Height = patient.Height,
                Weight = patient.Weight,
                InsuranceNumber = patient.InsuranceNumber,
                MedicalHistory = patient.MedicalHistory ?? new List<string>(),
                Allergies = patient.Allergies ?? new List<string>(),
                CreatedAt = patient.CreatedAt,
                UpdatedAt = patient.UpdatedAt
            };
        }

        private static Gender ParseGender(string? gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                throw new ArgumentException("Gender cannot be null or empty");

            if (Enum.TryParse<Gender>(gender, true, out var result))
                return result;

            throw new ArgumentException($"Invalid gender value: {gender}");
        }
    }
}