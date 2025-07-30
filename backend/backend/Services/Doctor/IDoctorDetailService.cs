using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models.Entities.Doctor;
using backend.Models.DTOs;


namespace backend.Services
{
    public interface IDoctorDetailService
    {
        Task<List<DoctorDetail>> GetAllAsync();

        Task<DoctorDetail?> GetDoctorDetailByDoctorIdAsync(string doctorId);
        Task<DoctorFullInfoDto?> GetDoctorFullInfoAsync(string doctorId);
        //   Task<DoctorDetail> CreateAsync(DoctorDetailUploadDto dto);


        Task<bool> UpdateAsync(string id, DoctorDetail updated);

        Task<bool> DeleteAsync(string id);
    }
}
