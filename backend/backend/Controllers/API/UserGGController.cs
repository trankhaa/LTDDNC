using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google; // Cần thiết cho GoogleDefaults
using Microsoft.AspNetCore.Identity; // Cần thiết cho IdentityConstants
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Services.GoogleAuth; // Namespace UserGGService của bạn
using backend.Models.Entities;     // Namespace User entity của bạn
using backend.Models.DTOs;       // Namespace Userlogin DTO của bạn
using Microsoft.Extensions.Configuration; // Cho IConfiguration

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserGGService _userService;
    private readonly IConfiguration _configuration;

    public AuthController(UserGGService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Userlogin loginData)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.GetByEmail(loginData.Email!);
        if (user == null || user.Password != loginData.Password)
            return Unauthorized(new { message = "Invalid email or password." });

        // Tạo Claims để đăng nhập với Cookie Authentication
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.Name ?? "")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Ok(new { message = "Login successful", user.Id, user.Name, user.Email });
    }

    // GET api/auth/login-google
    [HttpGet("login-google")]
    public IActionResult LoginGoogle()
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback));
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl! };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpPost("process-google-signin")] // Đặt tên route cho endpoint mới, ví dụ: "xu-ly-dang-nhap-google"
    public async Task<IActionResult> ProcessGoogleSignIn([FromBody] GoogleLoginDataDto googleLoginData)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 1. Tìm người dùng bằng GoogleId
        var user = await _userService.GetByGoogleId(googleLoginData.GoogleId);

        if (user == null)
        {
            // 2. Nếu không tìm thấy bằng GoogleId, thử tìm bằng Email (để liên kết tài khoản nếu email đã tồn tại)
            user = await _userService.GetByEmail(googleLoginData.Email);
            if (user != null)
            {
                // Người dùng tồn tại với email này, nhưng chưa có GoogleId hoặc GoogleId khác
                // Cập nhật GoogleId cho tài khoản hiện tại
                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    user.GoogleId = googleLoginData.GoogleId;
                    if (!string.IsNullOrEmpty(googleLoginData.Name) && user.Name != googleLoginData.Name)
                    {
                        user.Name = googleLoginData.Name; // Cập nhật tên nếu có và khác
                    }
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userService.UpdateUser(user);
                }
                else if (user.GoogleId != googleLoginData.GoogleId)
                {
                    // Xung đột: Email này đã được liên kết với một tài khoản Google khác.
                    // Tùy bạn xử lý: báo lỗi, hoặc cho phép ghi đè (cẩn thận)
                    return Conflict(new { message = "Email này đã được liên kết với một tài khoản Google khác." });
                }
                // Nếu user.GoogleId == googleLoginData.GoogleId, nghĩa là đã tìm thấy ở bước 1, hoặc email khớp và googleId cũng khớp.
            }
            else
            {
                // 3. Nếu không tìm thấy cả GoogleId và Email, tạo người dùng mới
                user = new User
                {
                    GoogleId = googleLoginData.GoogleId,
                    Email = googleLoginData.Email,
                    Name = googleLoginData.Name,
                    // Khi đăng ký bằng Google, Password có thể không cần thiết hoặc đặt một giá trị ngẫu nhiên an toàn
                    Password = Guid.NewGuid().ToString("N"), // Mật khẩu tạm, không dùng để đăng nhập thường
                    Role = "Patient", // Vai trò mặc định
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _userService.CreateUser(user);
            }
        }
        else
        {
            // 4. Người dùng đã tồn tại với GoogleId này, cập nhật thông tin nếu cần
            bool hasChanges = false;
            if (!string.IsNullOrEmpty(googleLoginData.Name) && user.Name != googleLoginData.Name)
            {
                user.Name = googleLoginData.Name;
                hasChanges = true;
            }
            // Có thể bạn muốn đồng bộ email nếu nó thay đổi từ Google
            if (user.Email != googleLoginData.Email)
            {
                // Cẩn thận khi thay đổi email, có thể cần xác minh lại
                // user.Email = googleLoginData.Email;
                // hasChanges = true;
            }

            if (hasChanges)
            {
                user.UpdatedAt = DateTime.UtcNow;
                await _userService.UpdateUser(user);
            }
        }

        // 5. Đăng nhập người dùng (tương tự như trong hàm GoogleCallback của bạn)
        // Bạn có thể chọn trả về Cookie hoặc JWT tùy theo cấu hình chính của ứng dụng
        // Ví dụ: Đăng nhập bằng Cookie
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
        new Claim(ClaimTypes.Name, user.Name ?? string.Empty),
        new Claim(ClaimTypes.Role, user.Role) // Thêm Role nếu bạn dùng
    };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Ok(new { message = "Đăng nhập bằng Google thành công!", user.Id, user.Name, user.Email });

        // Hoặc nếu bạn dùng JWT làm chính:
        // var token = GenerateJwtToken(user); // Hàm tạo JWT của bạn
        // return Ok(new { token, user.Id, user.Name, user.Email });
    }

    // GET api/auth/callback
    [HttpGet("callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
            return Unauthorized();

        var claims = authenticateResult.Principal.Claims;
        var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        if (googleId == null || email == null)
            return BadRequest("Google authentication failed.");

        var user = await _userService.GetByGoogleId(googleId);
        if (user == null)
        {
            user = new User
            {
                GoogleId = googleId,
                Email = email,
                Name = name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _userService.CreateUser(user);
        }
        else
        {
            user.Name = name;
            user.UpdatedAt = DateTime.UtcNow;
            await _userService.UpdateUser(user);
        }

        // Đăng nhập user bằng cookie
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.Name ?? "")
        }, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return Ok(new { user.Id, user.Name, user.Email });
    }

    // GET api/auth/user
    [HttpGet("user")]
    public async Task<IActionResult> GetUser()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = await _userService.GetById(userId);
            if (user == null)
                return Unauthorized();

            return Ok(user);
        }
        return Unauthorized();
    }

}