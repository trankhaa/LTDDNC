using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models.Entities.Doctor;
using backend.Services;
using backend.Models.DTOs;
using System.Threading.Tasks;


namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly DoctorService _doctorService;
        private readonly IDoctorDetailService _doctorDetailService;
        private readonly PasswordHasher<Doctor> _passwordHasher;

        public DoctorController(DoctorService doctorService, IDoctorDetailService doctorDetailService)
        {
            _doctorService = doctorService;
            _doctorDetailService = doctorDetailService;
            _passwordHasher = new PasswordHasher<Doctor>();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Doctor doctor)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Kiểm tra trùng email hoặc số điện thoại
            if (await _doctorService.IsDuplicateAsync(doctor.Email, doctor.Phone))
                return Conflict(new { message = "Email or phone number already exists." });

            doctor.Password = _passwordHasher.HashPassword(doctor, doctor.Password);
            doctor.CreatedAt = DateTime.UtcNow;
            doctor.UpdatedAt = DateTime.UtcNow;

            await _doctorService.AddAsync(doctor);

            // Tránh trả về thông tin nhạy cảm
            doctor.Password = string.Empty;

            return Ok(new { message = "Registration successful", doctor });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _doctorService.GetAllAsync();

            // Xoá mật khẩu khi trả về danh sách
            doctors.ForEach(d => d.Password = string.Empty);

            return Ok(doctors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] DoctorLogin loginData)
        {
            if (loginData == null || string.IsNullOrEmpty(loginData.Email))
                return BadRequest("Email is required.");

            var doctor = await _doctorService.GetByEmailAsync(loginData.Email);
            if (doctor == null)
                return Unauthorized("Invalid email or password.");

            var result = _passwordHasher.VerifyHashedPassword(doctor, doctor.Password, loginData.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid email or password.");

            var token = GenerateJwtToken(doctor);

            return Ok(new
            {
                message = "Login successful",
                token
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Nếu có sử dụng refresh token, bạn có thể xoá ở đây
            return Ok(new { message = "Logged out successfully. Please remove the token on the client." });
        }
        [HttpGet("all-with-details")]
        public async Task<IActionResult> GetAllDoctorsWithDetails()
        {
            try
            {
                var doctors = await _doctorService.GetAllAsync();
                var result = new List<object>();

                foreach (var doctor in doctors)
                {
                    var doctorDetail = await _doctorDetailService.GetDoctorDetailByDoctorIdAsync(doctor.IdDoctor);
                    result.Add(new
                    {
                        Id = doctor.IdDoctor,
                        Name = doctor.Name, // Đảm bảo luôn có trường Name
                        Email = doctor.Email,
                        Phone = doctor.Phone,
                        Gender = doctor.Gender,
                        DateOfBirth = doctor.DateOfBirth,
                        Cccd = doctor.Cccd,
                        Degree = doctorDetail?.Degree,
                        Description = doctorDetail?.Description,
                        Img = doctorDetail?.Img,
                        BranchName = doctorDetail?.BranchName,
                        DepartmentName = doctorDetail?.DepartmentName,
                        SpecialtyName = doctorDetail?.SpecialtyName
                    });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching doctors with details.", error = ex.Message });
            }
        }
        private string GenerateJwtToken(Doctor doctor)

        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, doctor.IdDoctor),
                new Claim(ClaimTypes.Name, doctor.Name),
                new Claim(ClaimTypes.Email, doctor.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key-should-be-at-least-16-characters-long"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(30);

            var token = new JwtSecurityToken(
                issuer: "your-app",
                audience: "your-app-Doctors",
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("{doctorId}/fullinfo")]
        public async Task<ActionResult<DoctorFullInfoDto>> GetDoctorFullInfo(string doctorId)
        {
            var doctorFullInfo = await _doctorDetailService.GetDoctorFullInfoAsync(doctorId);

            if (doctorFullInfo == null)
            {
                return NotFound(new { message = $"Doctor with ID {doctorId} not found." });
            }


            return Ok(doctorFullInfo);
        }
    }
}