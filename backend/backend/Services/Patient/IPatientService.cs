using backend.Models.Entities;
using backend.ViewModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace backend.Services.Patient
{
    public interface IPatientService
    {
        Task<List<PatientViewModel>> GetAllPatientsAsync();
        Task<PatientViewModel> GetPatientByIdAsync(string id);
        Task<PatientViewModel> CreatePatientAsync(PatientCreateViewModel model);
        Task UpdatePatientAsync(string id, PatientUpdateViewModel model);
        Task DeletePatientAsync(string id);
    }
}
