using MongoDB.Driver;
using backend.Models.Entities.Doctor;
using backend.Models.Entities;
using Microsoft.Extensions.Options;
using backend.Settings;
using Microsoft.AspNetCore.Identity;
using backend.Models.DTOs.Doctor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Bson;

namespace backend.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoCollection<Doctor> _doctorCollection;
        private readonly IMongoCollection<DoctorDetail> _doctorDetailCollection;
        private readonly IMongoCollection<DoctorSchedule> _doctorScheduleCollection;
        private readonly IMongoCollection<Branch> _branchCollection;
        private readonly IMongoCollection<Department> _departmentCollection;
        private readonly IMongoCollection<Specialty> _specialtyCollection;
        private readonly PasswordHasher<Doctor> _passwordHasher;
        private readonly IWebHostEnvironment _env;

        public DoctorService(IOptions<MongoDbSettings> mongoDbSettings, IWebHostEnvironment env, IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
            var mongoDatabase = _mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);

            _doctorCollection = mongoDatabase.GetCollection<Doctor>(mongoDbSettings.Value.DoctorCollectionName);
            _doctorDetailCollection = mongoDatabase.GetCollection<DoctorDetail>(mongoDbSettings.Value.DoctorDetailCollectionName);
            _doctorScheduleCollection = mongoDatabase.GetCollection<DoctorSchedule>(mongoDbSettings.Value.DoctorScheduleCollectionName);
            _branchCollection = mongoDatabase.GetCollection<Branch>(mongoDbSettings.Value.BranchCollectionName);
            _departmentCollection = mongoDatabase.GetCollection<Department>(mongoDbSettings.Value.DepartmentCollectionName);
            _specialtyCollection = mongoDatabase.GetCollection<Specialty>(mongoDbSettings.Value.SpecialtyCollectionName);

            _passwordHasher = new PasswordHasher<Doctor>();
            _env = env;
        }

        public async Task<List<Doctor>> GetAllAsync() =>
            await _doctorCollection.Find(_ => true).ToListAsync();

        public async Task AddAsync(Doctor newDoctor) =>
            await _doctorCollection.InsertOneAsync(newDoctor);

        public async Task RegisterDoctorAsync(Doctor newDoctor)
        {
            newDoctor.Password = _passwordHasher.HashPassword(newDoctor, newDoctor.Password);
            await _doctorCollection.InsertOneAsync(newDoctor);
        }

        public async Task<bool> IsDuplicateAsync(string email, string phone)
        {
            var count = await _doctorCollection.CountDocumentsAsync(u =>
                u.Email == email || u.Phone == phone);
            return count > 0;
        }

        public async Task<Doctor?> GetByEmailAsync(string email)
        {
            return await _doctorCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public bool VerifyPassword(Doctor doctor, string providedPassword)
        {
            var result = _passwordHasher.VerifyHashedPassword(doctor, doctor.Password!, providedPassword);
            return result == PasswordVerificationResult.Success;
        }

        private async Task<string> SaveImageAsync(IFormFile? file, string subFolder)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            var uploadsFolderPath = Path.Combine(_env.WebRootPath, "uploads", "doctors", subFolder);
            Directory.CreateDirectory(uploadsFolderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return $"/uploads/doctors/{subFolder}/{fileName}";
        }

        public async Task<(bool Success, string? ErrorMessage, Doctor? CreatedDoctor)> CreateDoctorWithDetailsAsync(CreateFullDoctorDto dto)
        {
            if (await IsDuplicateAsync(dto.Email, dto.Phone))
            {
                return (false, "Email hoặc số điện thoại đã được sử dụng.", null);
            }

            Branch? branch = null;
            if (ObjectId.TryParse(dto.BranchId, out _))
            {
                branch = await _branchCollection.Find(b => b.IdBranch == dto.BranchId).FirstOrDefaultAsync();
                if (branch == null) return (false, $"Không tìm thấy chi nhánh với ID: {dto.BranchId}", null);
            }
            else return (false, "BranchId không hợp lệ.", null);

            Department? department = null;
            if (ObjectId.TryParse(dto.DepartmentId, out _))
            {
                department = await _departmentCollection.Find(d => d.IdDepartment == dto.DepartmentId).FirstOrDefaultAsync();
                if (department == null) return (false, $"Không tìm thấy khoa với ID: {dto.DepartmentId}", null);
            }
            else return (false, "DepartmentId không hợp lệ.", null);

            Specialty? specialty = null;
            if (!string.IsNullOrEmpty(dto.SpecialtyId))
            {
                if (ObjectId.TryParse(dto.SpecialtyId, out var parsedSpecialtyId))
                {
                    specialty = await _specialtyCollection.Find(s => s.IdSpecialty == parsedSpecialtyId.ToString()).FirstOrDefaultAsync();
                    if (specialty == null) return (false, $"Không tìm thấy chuyên khoa với ID: {dto.SpecialtyId}", null);
                }
                else return (false, "SpecialtyId không hợp lệ.", null);
            }

            try
            {
                var newDoctor = new Doctor
                {
                    Name = dto.Name,
                    Gender = (backend.Models.Entities.Doctor.DoctorGender)dto.Gender,

                    DateOfBirth = dto.DateOfBirth.ToLocalTime(),
                    Cccd = dto.Cccd,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Password = _passwordHasher.HashPassword(null!, dto.Password),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _doctorCollection.InsertOneAsync(newDoctor);

                string imgUrl = await SaveImageAsync(dto.ImgFile, "avatars");
                string certificateImgUrl = await SaveImageAsync(dto.CertificateImgFile, "certificates");
                string degreeImgUrl = await SaveImageAsync(dto.DegreeImgFile, "degrees");

                var doctorDetail = new DoctorDetail
                {
                    DoctorId = newDoctor.IdDoctor,
                    Img = imgUrl,
                    CertificateImg = certificateImgUrl,
                    DegreeImg = degreeImgUrl,
                    Degree = dto.Degree,
                    Description = dto.Description,
                    BranchId = branch!.IdBranch,
                    DepartmentId = department!.IdDepartment,
                    SpecialtyId = specialty?.IdSpecialty,
                    BranchName = branch.BranchName,
                    DepartmentName = department.DepartmentName,
                    SpecialtyName = specialty?.SpecialtyName ?? string.Empty,
                    Branch = branch.IdBranch,
                    Department = department.IdDepartment,
                    Specialty = specialty?.IdSpecialty ?? string.Empty,
                };
                await _doctorDetailCollection.InsertOneAsync(doctorDetail);

                if (dto.ConsultationFee.HasValue && !string.IsNullOrEmpty(dto.StartTime) &&
                    !string.IsNullOrEmpty(dto.EndTime) && dto.ExaminationTime.HasValue)
                {
                    var doctorSchedule = new DoctorSchedule
                    {
                        DoctorId = newDoctor.IdDoctor,
                        ConsultationFee = dto.ConsultationFee.Value,
                        StartTime = dto.StartTime,
                        EndTime = dto.EndTime,
                        ExaminationTime = dto.ExaminationTime.Value
                    };
                    await _doctorScheduleCollection.InsertOneAsync(doctorSchedule);
                }

                return (true, "Bác sĩ và thông tin chi tiết đã được tạo thành công.", newDoctor);
            }
            catch (global::MongoDB.Driver.MongoException ex)
            {
                return (false, $"Lỗi từ cơ sở dữ liệu khi tạo bác sĩ: {ex.Message}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Đã xảy ra lỗi không mong muốn: {ex.Message}", null);
            }
        }


        public async Task<Doctor?> GetByIdAsync(string doctorId)
        {
            return await _doctorCollection.Find(d => d.IdDoctor == doctorId).FirstOrDefaultAsync();
        }


        private void DeleteImageFromServer(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            try
            {
                // imageUrl thường là /uploads/doctors/avatars/guid.jpg
                // Cần chuyển nó thành đường dẫn tuyệt đối trên server
                var webRootPath = _env.WebRootPath;
                var filePath = Path.Combine(webRootPath, imageUrl.TrimStart('/')); // Loại bỏ dấu '/' ở đầu nếu có

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // Log lỗi khi xóa file, nhưng không nên để nó làm dừng tiến trình cập nhật
                Console.WriteLine($"Error deleting file {imageUrl}: {ex.Message}"); // Thay bằng logger thực tế
            }
        }






        public async Task<bool> IsEmailOrPhoneDuplicateAsync(string email, string phone, string? excludeDoctorId = null)
        {
            var filterBuilder = Builders<Doctor>.Filter;
            var orFilter = filterBuilder.Or(
                filterBuilder.Eq(u => u.Email, email),
                filterBuilder.Eq(u => u.Phone, phone)
            );

            FilterDefinition<Doctor> finalFilter = orFilter;

            if (!string.IsNullOrEmpty(excludeDoctorId))
            {
                var excludeFilter = filterBuilder.Ne(u => u.IdDoctor, excludeDoctorId);
                finalFilter = filterBuilder.And(orFilter, excludeFilter);
            }

            var count = await _doctorCollection.CountDocumentsAsync(finalFilter);
            return count > 0;
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateDoctorWithDetailsAsync(string doctorId, UpdateFullDoctorDto dto)
        {
            var transactionOptions = new TransactionOptions(
                readConcern: ReadConcern.Snapshot,
                writeConcern: WriteConcern.WMajority
            );

            using var session = await _mongoClient.StartSessionAsync();


            try
            {
                // 1. Lấy và cập nhật Doctor
                var doctorFilter = Builders<Doctor>.Filter.Eq(d => d.IdDoctor, doctorId);
                var doctor = await _doctorCollection.Find(session, doctorFilter).FirstOrDefaultAsync();
                if (doctor == null)
                {
                    await session.AbortTransactionAsync();
                    return (false, "Không tìm thấy bác sĩ để cập nhật.");
                }

                if ((doctor.Email != dto.Email || doctor.Phone != dto.Phone) &&
                    await IsEmailOrPhoneDuplicateAsync(dto.Email, dto.Phone, doctorId))
                {
                    await session.AbortTransactionAsync();
                    return (false, "Email hoặc số điện thoại đã được sử dụng bởi người khác.");
                }

                doctor.Name = dto.Name;
                doctor.Gender = dto.Gender; // Giả sử DoctorGender đã được hợp nhất kiểu
                doctor.DateOfBirth = dto.DateOfBirth.ToLocalTime();
                doctor.Cccd = dto.Cccd;
                doctor.Phone = dto.Phone;
                doctor.Email = dto.Email;
                doctor.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(dto.NewPassword))
                {
                    doctor.Password = _passwordHasher.HashPassword(doctor, dto.NewPassword);
                }
                await _doctorCollection.ReplaceOneAsync(session, doctorFilter, doctor);

                // 2. Lấy và cập nhật DoctorDetail
                var detailFilter = Builders<DoctorDetail>.Filter.Eq(dd => dd.DoctorId, doctorId);
                var doctorDetail = await _doctorDetailCollection.Find(session, detailFilter).FirstOrDefaultAsync();
                if (doctorDetail == null)
                {
                    await session.AbortTransactionAsync();
                    return (false, "Không tìm thấy thông tin chi tiết của bác sĩ.");
                }

                doctorDetail.Degree = dto.Degree;
                doctorDetail.Description = dto.Description;

                if (dto.ImgFile != null && dto.ImgFile.Length > 0)
                {
                    DeleteImageFromServer(doctorDetail.Img);
                    doctorDetail.Img = await SaveImageAsync(dto.ImgFile, "avatars");
                }
                if (dto.CertificateImgFile != null && dto.CertificateImgFile.Length > 0)
                {
                    DeleteImageFromServer(doctorDetail.CertificateImg);
                    doctorDetail.CertificateImg = await SaveImageAsync(dto.CertificateImgFile, "certificates");
                }
                if (dto.DegreeImgFile != null && dto.DegreeImgFile.Length > 0)
                {
                    DeleteImageFromServer(doctorDetail.DegreeImg);
                    doctorDetail.DegreeImg = await SaveImageAsync(dto.DegreeImgFile, "degrees");
                }

                if (doctorDetail.BranchId != dto.BranchId && !string.IsNullOrEmpty(dto.BranchId))
                {
                    var newBranch = await _branchCollection.Find(session, b => b.IdBranch == dto.BranchId).FirstOrDefaultAsync();
                    if (newBranch == null) { await session.AbortTransactionAsync(); return (false, $"Chi nhánh ID {dto.BranchId} không tồn tại."); }
                    doctorDetail.BranchId = newBranch.IdBranch;
                    doctorDetail.BranchName = newBranch.BranchName;
                    doctorDetail.Branch = newBranch.IdBranch;
                }
                if (doctorDetail.DepartmentId != dto.DepartmentId && !string.IsNullOrEmpty(dto.DepartmentId))
                {
                    var newDepartment = await _departmentCollection.Find(session, d => d.IdDepartment == dto.DepartmentId).FirstOrDefaultAsync();
                    if (newDepartment == null) { await session.AbortTransactionAsync(); return (false, $"Khoa ID {dto.DepartmentId} không tồn tại."); }
                    doctorDetail.DepartmentId = newDepartment.IdDepartment;
                    doctorDetail.DepartmentName = newDepartment.DepartmentName;
                    doctorDetail.Department = newDepartment.IdDepartment;
                }
                if (doctorDetail.SpecialtyId != dto.SpecialtyId)
                {
                    if (!string.IsNullOrEmpty(dto.SpecialtyId))
                    {
                        var newSpecialty = await _specialtyCollection.Find(session, s => s.IdSpecialty == dto.SpecialtyId).FirstOrDefaultAsync();
                        if (newSpecialty == null) { await session.AbortTransactionAsync(); return (false, $"Chuyên khoa ID {dto.SpecialtyId} không tồn tại."); }
                        doctorDetail.SpecialtyId = newSpecialty.IdSpecialty;
                        doctorDetail.SpecialtyName = newSpecialty.SpecialtyName;
                        doctorDetail.Specialty = newSpecialty.IdSpecialty;
                    }
                    else
                    {
                        doctorDetail.SpecialtyId = null;
                        doctorDetail.SpecialtyName = string.Empty;
                        doctorDetail.Specialty = string.Empty;
                    }
                }
                await _doctorDetailCollection.ReplaceOneAsync(session, detailFilter, doctorDetail);

                // 3. Cập nhật hoặc tạo DoctorSchedule
                var scheduleFilter = Builders<DoctorSchedule>.Filter.Eq(ds => ds.DoctorId, doctorId);
                var schedule = await _doctorScheduleCollection.Find(session, scheduleFilter).FirstOrDefaultAsync();

                bool hasScheduleDataInDto = dto.ConsultationFee.HasValue ||
                                            !string.IsNullOrEmpty(dto.StartTime) ||
                                            !string.IsNullOrEmpty(dto.EndTime) ||
                                            dto.ExaminationTime.HasValue;

                if (hasScheduleDataInDto)
                {
                    if (schedule == null)
                    {
                        schedule = new DoctorSchedule { DoctorId = doctorId };
                        // Giả định DoctorSchedule.ConsultationFee là int, dto.ConsultationFee là decimal?
                        schedule.ConsultationFee = dto.ConsultationFee.HasValue ? (int)dto.ConsultationFee.Value : 0;
                        schedule.StartTime = dto.StartTime ?? string.Empty;
                        schedule.EndTime = dto.EndTime ?? string.Empty;
                        // Giả định DoctorSchedule.ExaminationTime là int, dto.ExaminationTime là int?
                        schedule.ExaminationTime = dto.ExaminationTime.HasValue ? dto.ExaminationTime.Value : 0;
                        await _doctorScheduleCollection.InsertOneAsync(session, schedule);
                    }
                    else
                    {
                        // Giả định DoctorSchedule.ConsultationFee là int, dto.ConsultationFee là decimal?
                        if (dto.ConsultationFee.HasValue)
                        {
                            schedule.ConsultationFee = (int)dto.ConsultationFee.Value;
                        }
                        // else: schedule.ConsultationFee giữ nguyên giá trị cũ

                        schedule.StartTime = !string.IsNullOrEmpty(dto.StartTime) ? dto.StartTime : schedule.StartTime;
                        schedule.EndTime = !string.IsNullOrEmpty(dto.EndTime) ? dto.EndTime : schedule.EndTime;

                        // Giả định DoctorSchedule.ExaminationTime là int, dto.ExaminationTime là int?
                        if (dto.ExaminationTime.HasValue)
                        {
                            schedule.ExaminationTime = dto.ExaminationTime.Value;
                        }
                        // else: schedule.ExaminationTime giữ nguyên giá trị cũ

                        await _doctorScheduleCollection.ReplaceOneAsync(session, scheduleFilter, schedule);
                    }
                }


                return (true, null);
            }
            catch (MongoException ex)
            {
                await session.AbortTransactionAsync();
                Console.WriteLine($"MongoDB Exception in UpdateDoctorWithDetailsAsync for DoctorID {doctorId}: {ex.ToString()}");
                return (false, $"Lỗi cơ sở dữ liệu khi cập nhật bác sĩ. Vui lòng thử lại hoặc liên hệ quản trị viên.");
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                Console.WriteLine($"Generic Exception in UpdateDoctorWithDetailsAsync for DoctorID {doctorId}: {ex.ToString()}");
                return (false, $"Lỗi hệ thống không mong muốn khi cập nhật bác sĩ. Vui lòng thử lại hoặc liên hệ quản trị viên.");
            }
        }     // và IDoctorService có khai báo các phương thức này.
              // Ví dụ:
        public async Task<Doctor?> GetDoctorByIdAsync(string doctorId) =>
            await _doctorCollection.Find(d => d.IdDoctor == doctorId).FirstOrDefaultAsync();

        public async Task<List<Doctor>> GetAllDoctorsBasicInfoAsync() =>
            await _doctorCollection.Find(_ => true).ToListAsync();

        public async Task<Doctor?> GetDoctorByEmailAsync(string email) =>
        await _doctorCollection.Find(d => d.Email == email).FirstOrDefaultAsync();

        public bool VerifyDoctorPassword(Doctor doctor, string providedPassword)
        {
            if (doctor == null || string.IsNullOrEmpty(doctor.Password)) return false;
            var result = _passwordHasher.VerifyHashedPassword(doctor, doctor.Password, providedPassword);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<int> CountDoctorsAsync()
        {
            return (int)await _doctorCollection.CountDocumentsAsync(_ => true);
        }


        public async Task<(bool Success, string? ErrorMessage)> DeleteDoctorAsync(string doctorId)
        {
            var transactionOptions = new TransactionOptions(
                readConcern: ReadConcern.Snapshot,
                writeConcern: WriteConcern.WMajority
            );

            using var session = await _mongoClient.StartSessionAsync();


            try
            {
                // 0. Kiểm tra bác sĩ có tồn tại không (và lấy thông tin cần để xóa ảnh)
                var doctor = await _doctorCollection.Find(session, d => d.IdDoctor == doctorId).FirstOrDefaultAsync();
                if (doctor == null)
                {
                    await session.AbortTransactionAsync(); // Không cần thiết nếu chỉ đọc, nhưng để nhất quán
                    return (false, "Không tìm thấy bác sĩ để xóa.");
                }

                var doctorDetail = await _doctorDetailCollection.Find(session, dd => dd.DoctorId == doctorId).FirstOrDefaultAsync();

                // 1. Xóa DoctorSchedule
                var scheduleFilter = Builders<DoctorSchedule>.Filter.Eq(ds => ds.DoctorId, doctorId);
                await _doctorScheduleCollection.DeleteManyAsync(session, scheduleFilter); // Xóa tất cả lịch của bác sĩ này

                // 2. Xóa DoctorDetail
                if (doctorDetail != null)
                {
                    var detailFilter = Builders<DoctorDetail>.Filter.Eq(dd => dd.DoctorId, doctorId);
                    await _doctorDetailCollection.DeleteOneAsync(session, detailFilter);
                }

                // 3. Xóa Doctor
                var doctorFilter = Builders<Doctor>.Filter.Eq(d => d.IdDoctor, doctorId);
                var deleteResult = await _doctorCollection.DeleteOneAsync(session, doctorFilter);

                if (deleteResult.DeletedCount == 0) // Kiểm tra lại nếu có lỗi logic nào đó
                {
                    await session.AbortTransactionAsync();
                    return (false, "Không thể xóa thông tin cơ bản của bác sĩ.");
                }

                // 4. Xóa ảnh từ server (thực hiện sau khi transaction thành công)
                // Nếu transaction thất bại, ảnh sẽ không bị xóa.
                // Nếu muốn xóa ảnh ngay cả khi transaction thất bại một phần (ví dụ DB lỗi sau khi xóa ảnh), logic sẽ phức tạp hơn.
                // Hiện tại: chỉ xóa ảnh nếu DB operations thành công.



                // Sau khi commit thành công, tiến hành xóa file
                if (doctorDetail != null)
                {
                    DeleteImageFromServer(doctorDetail.Img);
                    DeleteImageFromServer(doctorDetail.CertificateImg);
                    DeleteImageFromServer(doctorDetail.DegreeImg);
                }

                return (true, null);
            }
            catch (MongoException ex)
            {
                await session.AbortTransactionAsync();
                Console.WriteLine($"MongoDB Exception in DeleteDoctorAsync for DoctorID {doctorId}: {ex.ToString()}");
                return (false, $"Lỗi cơ sở dữ liệu khi xóa bác sĩ. Vui lòng thử lại hoặc liên hệ quản trị viên.");
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                Console.WriteLine($"Generic Exception in DeleteDoctorAsync for DoctorID {doctorId}: {ex.ToString()}");
                return (false, $"Lỗi hệ thống không mong muốn khi xóa bác sĩ. Vui lòng thử lại hoặc liên hệ quản trị viên.");
            }
        }
    }
}