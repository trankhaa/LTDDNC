using MongoDB.Driver;
using backend.Models.Entities.Doctor;
using Microsoft.Extensions.Options;
using backend.Data;

using backend.Models.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using MongoDB.Bson;
using System.Threading.Tasks;
using backend.Services;
using backend.Models.Entities;
using backend.Models.DTOs;
using backend.Settings;

namespace backend.Services
{
    public class DoctorDetailService : IDoctorDetailService
    {
        private readonly IMongoCollection<DoctorDetail> _DoctorDetailCollection;
        private readonly IMongoCollection<Branch> _BranchCollection;
        private readonly IMongoCollection<Doctor> _doctorCollection; // <<< THÊM LẠI DÒNG NÀY
        private readonly IMongoCollection<Department> _departmentCollection;
        private readonly IMongoCollection<Specialty> _specialtyCollection;
        private readonly IMongoCollection<DoctorSchedule> _DoctorScheduleCollectionName;
        private readonly IWebHostEnvironment _env;

        public DoctorDetailService(IOptions<MongoDbSettings> mongoDbSettings, IWebHostEnvironment env)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);

            _DoctorDetailCollection = mongoDatabase.GetCollection<DoctorDetail>(mongoDbSettings.Value.DoctorDetailCollectionName);
            _BranchCollection = mongoDatabase.GetCollection<Branch>(mongoDbSettings.Value.BranchCollectionName);
            _departmentCollection = mongoDatabase.GetCollection<Department>(mongoDbSettings.Value.DepartmentCollectionName);
            _specialtyCollection = mongoDatabase.GetCollection<Specialty>(mongoDbSettings.Value.SpecialtyCollectionName);
            _doctorCollection = mongoDatabase.GetCollection<Doctor>(mongoDbSettings.Value.DoctorCollectionName);
            _DoctorScheduleCollectionName = mongoDatabase.GetCollection<DoctorSchedule>(mongoDbSettings.Value.DoctorScheduleCollectionName);
            _env = env;
        }

        public async Task<List<DoctorDetail>> GetAllAsync() =>
            await _DoctorDetailCollection.Find(_ => true).ToListAsync();

        public async Task<DoctorDetail?> GetDoctorDetailByDoctorIdAsync(string doctorId) =>
            await _DoctorDetailCollection.Find(d => d.DoctorId == doctorId).FirstOrDefaultAsync();

        public async Task<(bool Success, string? ErrorMessage, DoctorDetail? Result)> CreateAsync(DoctorDetailUploadDto dto)
        {
            // Validate all IDs
            if (!ObjectId.TryParse(dto.BranchId, out var branchId))
                return (false, $"Invalid BranchId: {dto.BranchId}", null);

            if (!ObjectId.TryParse(dto.DepartmentId, out var departmentId))
                return (false, $"Invalid DepartmentId: {dto.DepartmentId}", null);

            if (!ObjectId.TryParse(dto.SpecialtyId, out var specialtyId))
                return (false, $"Invalid SpecialtyId: {dto.SpecialtyId}", null);

            if (!ObjectId.TryParse(dto.DoctorId, out var doctorId))
                return (false, $"Invalid DoctorId: {dto.DoctorId}", null);

            // Check Branch exists
            var branch = await _BranchCollection.Find(b => b.IdBranch == branchId.ToString()).FirstOrDefaultAsync();
            if (branch == null)
                return (false, $"Branch with id {dto.BranchId} not found", null);

            // Check Department exists
            var department = await _departmentCollection.Find(d => d.IdDepartment == departmentId.ToString()).FirstOrDefaultAsync();
            if (department == null)
                return (false, $"Department with id {dto.DepartmentId} not found", null);

            // Check Specialty exists
            var specialty = await _specialtyCollection.Find(s => s.IdSpecialty == specialtyId.ToString()).FirstOrDefaultAsync();
            if (specialty == null)
                return (false, $"Specialty with id {dto.SpecialtyId} not found", null);

            var doctorDetail = new DoctorDetail
            {
                DoctorId = doctorId.ToString(),
                Degree = dto.Degree,
                BranchId = branch.IdBranch,
                DepartmentId = department.IdDepartment,
                SpecialtyId = specialty.IdSpecialty,
                Description = dto.Description ?? string.Empty,
                Img = dto.ImgFile != null ? await SaveImageAsync(dto.ImgFile, "Doctor/Img") : string.Empty,
                CertificateImg = dto.CertificateImgFile != null ? await SaveImageAsync(dto.CertificateImgFile, "Doctor/CertificateImg") : string.Empty,
                DegreeImg = dto.DegreeImgFile != null ? await SaveImageAsync(dto.DegreeImgFile, "Doctor/DegreeImg") : string.Empty,
            };

            await _DoctorDetailCollection.InsertOneAsync(doctorDetail);
            return (true, null, doctorDetail);
        }

        public async Task<bool> UpdateAsync(string id, DoctorDetail updated)
        {
            var result = await _DoctorDetailCollection.ReplaceOneAsync(d => d.IdDoctorDetail == id, updated);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _DoctorDetailCollection.DeleteOneAsync(d => d.IdDoctorDetail == id);
            return result.IsAcknowledged && result.DeletedCount > 0;
        }

        private async Task<string> SaveImageAsync(IFormFile file, string folderName)
        {
            var folderPath = Path.Combine(_env.WebRootPath, "uploads", folderName);
            Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Trả về đường dẫn dạng URL có thể dùng ở client
            return $"/uploads/{folderName}/{fileName}";
        }

        public async Task<List<DoctorSearchResultDto>> FindDoctorsByCriteriaAsync(string branchId, string departmentId, string? specialtyId = null)
        {
            var matchFilter = new BsonDocument();
            // Chỉ thêm điều kiện nếu ID là ObjectId hợp lệ
            if (ObjectId.TryParse(branchId, out var parsedBranchId))
                matchFilter.Add("branchId", parsedBranchId);
            else
                // Lỗi: branchId không hợp lệ, bạn có thể throw exception hoặc trả về danh sách rỗng
                // Ví dụ: throw new ArgumentException("Invalid Branch ID format", nameof(branchId));
                return new List<DoctorSearchResultDto>(); // Hoặc xử lý lỗi theo cách của bạn

            if (ObjectId.TryParse(departmentId, out var parsedDepartmentId))
                matchFilter.Add("departmentId", parsedDepartmentId);
            else
                // Lỗi: departmentId không hợp lệ
                // Ví dụ: throw new ArgumentException("Invalid Department ID format", nameof(departmentId));
                return new List<DoctorSearchResultDto>(); // Hoặc xử lý lỗi theo cách của bạn


            if (!string.IsNullOrEmpty(specialtyId) && ObjectId.TryParse(specialtyId, out var parsedSpecialtyObjectId))
            {
                matchFilter.Add("specialtyId", parsedSpecialtyObjectId);
            }
            else if (specialtyId == null) // Client muốn tìm những người không có chuyên khoa cụ thể (specialtyId là null trong DB)
            {
                matchFilter.Add("specialtyId", BsonNull.Value);
            }
            // Nếu specialtyId là chuỗi rỗng nhưng không phải null, hiện tại sẽ không lọc theo specialtyId.

            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", matchFilter),
                new BsonDocument("$lookup",
                    new BsonDocument
                    {
                        { "from", _doctorCollection.CollectionNamespace.CollectionName },
                        { "let", new BsonDocument("doctor_id_str", "$doctorId") },
                        { "pipeline", new BsonArray
                            {
                                new BsonDocument("$match",
                                    new BsonDocument("$expr",
                                        new BsonDocument("$eq", new BsonArray { "$_id", new BsonDocument("$toObjectId", "$$doctor_id_str") })
                                    )
                                ),
                                new BsonDocument("$project", // Các trường từ collection Doctor
                                   new BsonDocument
    {
        { "_id", 1 },
        { "name", 1 },
        { "gender", 1 },      // sửa lại chữ thường và giá trị là 1
        { "dateOfBirth", 1 },
        { "cccd", 1 },
        { "phone", 1 },       // sửa thành chữ thường
        { "email", 1 }        // sửa thành chữ thường
    })
                            }
                        },
                        { "as", "doctorInfoArray" }
                    }),
                new BsonDocument("$unwind",
                    new BsonDocument
                    {
                        { "path", "$doctorInfoArray" },
                        { "preserveNullAndEmptyArrays", false }
                    }),
                new BsonDocument("$project", // Định dạng kết quả cuối cùng thành DoctorSearchResultDto
                    new BsonDocument
                    {
                        // Tên các trường ở đây PHẢI KHỚP với tên thuộc tính trong DoctorSearchResultDto
                       { "_id", "$doctorInfoArray._id" },
// Sửa "Id" thành "id"
                     { "Name", "$doctorInfoArray.name" },          // Phải trùng với DTO
        { "Gender", "$doctorInfoArray.gender" },
        { "DateOfBirth", "$doctorInfoArray.dateOfBirth" },
        { "Cccd", "$doctorInfoArray.cccd" },
        { "Phone", "$doctorInfoArray.phone" },
        { "Email", "$doctorInfoArray.email" },
                        { "DoctorDetailRecordId", "$_id" },
                        { "DoctorImage", "$img" },
                        { "DoctorDegree", "$degree" },
                        { "WorkingAtBranch", "$branch" },
                        { "WorkingInDepartment", "$department" },
                        { "SpecializingIn", "$specialty" },
                        { "JobDescription", "$description" },
                        { "BranchIdRef", "$branchId" },
                        { "DepartmentIdRef", "$departmentId" },
                        { "SpecialtyIdRef", "$specialtyId" }
                    })
            };

            // SỬA ĐỔI Ở ĐÂY: Kiểu Generic cho Aggregate
            return await _DoctorDetailCollection.Aggregate<DoctorSearchResultDto>(pipeline).ToListAsync();
        }

        public async Task<List<DoctorSearchResultDto>> SearchDoctorsByCriteria(SearchDoctorCriteriaDto criteria)
        {
            return await FindDoctorsByCriteriaAsync(
                criteria.BranchId,
                criteria.DepartmentId,
                criteria.SpecialtyId
            );
        }

        // ======================================================================================
        // === DÁN ĐOẠN CODE NÀY VÀO TRONG CLASS DoctorDetailService ===
        // ======================================================================================

        public async Task<DoctorFullInfoDto?> GetDoctorFullInfoAsync(string doctorId)
        {
            // Bước 1: Tìm bản ghi Doctor chính. Đây là thông tin bắt buộc.
            var doctor = await _doctorCollection.Find(d => d.IdDoctor == doctorId).FirstOrDefaultAsync();
            if (doctor == null)
            {
                // Nếu không có bản ghi Doctor, không thể tiếp tục. Trả về null.
                // Controller sẽ dựa vào đây để trả về 404 Not Found.
                Console.WriteLine($"GetDoctorFullInfoAsync: Doctor with ID '{doctorId}' not found in 'doctors' collection.");
                return null;
            }

            // Bước 2: Tìm bản ghi DoctorDetail. Đây cũng là thông tin bắt buộc.
            var doctorDetail = await _DoctorDetailCollection.Find(d => d.DoctorId == doctorId).FirstOrDefaultAsync();
            if (doctorDetail == null)
            {
                // Nếu không có chi tiết, thông tin sẽ không đầy đủ. Trả về null.
                Console.WriteLine($"GetDoctorFullInfoAsync: DoctorDetail for Doctor ID '{doctorId}' not found.");
                return null;
            }

            // Bước 3: Tìm tất cả lịch làm việc của bác sĩ.
            // Nếu không có lịch nào, sẽ trả về một danh sách rỗng, điều này là hợp lệ.
            var doctorSchedules = await _DoctorScheduleCollectionName.Find(d => d.DoctorId == doctorId).ToListAsync();

            // Bước 4: Lấy tên chi nhánh, khoa, chuyên khoa từ các ID tham chiếu.
            // Khởi tạo các biến tên, chúng sẽ vẫn là null nếu không tìm thấy ID tương ứng.
            string? branchName = null;
            string? departmentName = null;
            string? specialtyName = null;

            if (!string.IsNullOrEmpty(doctorDetail.BranchId))
            {
                var branch = await _BranchCollection.Find(b => b.IdBranch == doctorDetail.BranchId).FirstOrDefaultAsync();
                branchName = branch?.BranchName; // Sử dụng toán tử "?." để tránh lỗi nếu branch là null
            }

            if (!string.IsNullOrEmpty(doctorDetail.DepartmentId))
            {
                var department = await _departmentCollection.Find(d => d.IdDepartment == doctorDetail.DepartmentId).FirstOrDefaultAsync();
                departmentName = department?.DepartmentName;
            }

            if (!string.IsNullOrEmpty(doctorDetail.SpecialtyId))
            {
                var specialty = await _specialtyCollection.Find(s => s.IdSpecialty == doctorDetail.SpecialtyId).FirstOrDefaultAsync();
                specialtyName = specialty?.SpecialtyName;
            }

            // Bước 5: Tạo đối tượng DTO (Data Transfer Object) để trả về cho client.
            // Đối tượng này gom tất cả thông tin lại một chỗ.
            return new DoctorFullInfoDto
            {
                Doctor = doctor,
                DoctorDetail = doctorDetail,
                DoctorSchedules = doctorSchedules,
                BranchName = branchName,
                DepartmentName = departmentName,
                SpecialtyName = specialtyName
            };
        }



    }
}