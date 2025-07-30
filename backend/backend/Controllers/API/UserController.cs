using Microsoft.AspNetCore.Mvc;
using backend.Models.DTOs;
using backend.Models.Entities;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using backend.Helper;

namespace backend.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMongoDatabase _database;
        private readonly IBcryptHelper _bcryptHelper;
        
        public UserController(IMongoDatabase database, IBcryptHelper bcryptHelper)
        {
            _database = database;
            _bcryptHelper = bcryptHelper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check duplicate email
            var usersCollection = _database.GetCollection<User>("Users");
            var patientsCollection = _database.GetCollection<Patient>("Patients");
            
            var existingUser = await usersCollection.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
            if (existingUser != null)
                return Conflict(new { message = "Email đã tồn tại." });

            // Create Patient
            var patient = new Patient
            {
                FullName = dto.Name ?? string.Empty,
                Gender = dto.Gender.HasValue ? (Gender)dto.Gender.Value : Gender.Other,
                DateOfBirth = dto.DateOfBirth ?? DateTime.UtcNow.AddYears(-dto.Age),
                Phone = dto.Phone ?? string.Empty,
                Address = dto.Address ?? string.Empty,
                PatientCode = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await patientsCollection.InsertOneAsync(patient);

            // Create User
            var user = new User
            {
                Email = dto.Email!,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password!),
                Role = "Patient",
                IsActive = true,
                PatientId = patient.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await usersCollection.InsertOneAsync(user);

            // Update patient with userId
            var update = Builders<Patient>.Update.Set(p => p.UserId, user.Id);
            await patientsCollection.UpdateOneAsync(p => p.Id == patient.Id, update);

            return Ok(new { message = "Đăng ký thành công", user = new { user.Id, user.Email }, patient = new { patient.Id, patient.FullName } });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find user by email
            var usersCollection = _database.GetCollection<User>("Users");
            var patientsCollection = _database.GetCollection<Patient>("Patients");
            
            var user = await usersCollection.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
            if (user == null)
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

            // Verify password
            if (!_bcryptHelper.VerifyPassword(dto.Password!, user.Password))
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

            // Get patient information
            var patient = await patientsCollection.Find(p => p.UserId == user.Id).FirstOrDefaultAsync();

            return Ok(new { 
                message = "Đăng nhập thành công",
                id = user.Id,
                email = user.Email,
                name = patient?.FullName ?? user.Name,
                phone = patient?.Phone,
                role = user.Role
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserProfile(string id)
        {
            var usersCollection = _database.GetCollection<User>("Users");
            var patientsCollection = _database.GetCollection<Patient>("Patients");
            
            var user = await usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null) return NotFound();
            var patient = await patientsCollection.Find(p => p.UserId == id).FirstOrDefaultAsync();
            return Ok(new {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                phone = patient?.Phone,
                dateOfBirth = patient?.DateOfBirth,
                gender = patient?.Gender,
                fullName = patient?.FullName
            });
        }

        [HttpGet("{id}/bookings")]
        public async Task<IActionResult> GetUserBookings(string id)
        {
            // Giả lập dữ liệu booking, bạn thay bằng truy vấn thực tế nếu có
            var bookings = new[] {
                new { date = DateTime.UtcNow.AddDays(-1), doctorName = "BS. Nguyễn Văn A", specialtyName = "Nội tổng quát", status = "Hoàn thành" },
                new { date = DateTime.UtcNow.AddDays(-10), doctorName = "BS. Trần Thị B", specialtyName = "Tai Mũi Họng", status = "Đã hủy" }
            };
            return Ok(bookings);
        }

        [HttpGet("{id}/packages")]
        public async Task<IActionResult> GetUserPackages(string id)
        {
            // Giả lập dữ liệu gói khám, bạn thay bằng truy vấn thực tế nếu có
            var packages = new[] {
                new { packageName = "Gói khám tổng quát", description = "Khám sức khỏe tổng quát định kỳ" },
                new { packageName = "Gói khám chuyên sâu", description = "Khám chuyên sâu tim mạch" }
            };
            return Ok(packages);
        }
    }
} 