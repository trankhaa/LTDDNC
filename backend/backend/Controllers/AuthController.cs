// // --- File: backend/Controllers/AuthController.cs ---
// using backend.Models.Entities;       // Cho GoogleUser
// using backend.Services.GoogleAuth;   // Cho IGoogleAuthService

// using backend.Models.DTOs;                  // Cho GoogleSignInRequest, GoogleUserDto
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using System;
// using System.Collections.Generic;    // Cho List<Claim>
// using System.Security.Claims;        // Cho ClaimsIdentity, Claim, ClaimTypes, ClaimsPrincipal
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Authentication; // Cho HttpContext.SignInAsync/SignOutAsync
// using Microsoft.AspNetCore.Authentication.Cookies; // Cho CookieAuthenticationDefaults
// using Microsoft.AspNetCore.Authorization; // Cho [Authorize] nếu cần cho các endpoint khác

// namespace backend.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class AuthController : ControllerBase
//     {
//         private readonly IGoogleAuthService _googleAuthService;
//         private readonly IGoogleUserService _googleUserService;
//         private readonly ILogger<AuthController> _logger;

//         public AuthController(
//             IGoogleAuthService googleAuthService,
//             IGoogleUserService googleUserService,
//             ILogger<AuthController> logger)
//         {
//             _googleAuthService = googleAuthService;
//             _googleUserService = googleUserService;
//             _logger = logger;
//         }

//         [HttpPost("google-signin")]
//         public async Task<IActionResult> GoogleSignIn([FromBody] GoogleSignInRequest request)
//         {
//             if (string.IsNullOrWhiteSpace(request.AuthorizationCode))
//             {
//                 _logger.LogWarning("GoogleSignIn: Authorization code is missing.");
//                 return BadRequest(new { message = "Authorization code is required." });
//             }

//             // AuthController.cs
//             // ...
//             var googlePayload = await _googleAuthService.VerifyGoogleTokenAsync(request.AuthorizationCode);

//             if (googlePayload == null)
//             {
//                 _logger.LogWarning("GoogleSignIn: Failed to verify Google token or code was invalid.");
//                 // LỖI RẤT CÓ THỂ PHÁT SINH TỪ ĐÂY
//                 return Unauthorized(new { message = "Invalid Google authorization code or failed to verify." });
//             }
//             // ...

//             _logger.LogInformation("GoogleSignIn: Google token verified. User Email: {Email}, GoogleID: {GoogleID}", googlePayload.Email, googlePayload.Subject);

//             GoogleUser? user = await _googleUserService.GetUserByGoogleIdAsync(googlePayload.Subject);

//             if (user == null)
//             {
//                 _logger.LogInformation("GoogleSignIn: User not found by GoogleID. Checking by email: {Email}", googlePayload.Email);
//                 user = await _googleUserService.GetUserByEmailAsync(googlePayload.Email);

//                 if (user != null)
//                 {
//                     _logger.LogInformation("GoogleSignIn: User found by email. ID: {UserId}. Linking Google account.", user.Id);
//                     if (string.IsNullOrEmpty(user.GoogleId))
//                     {
//                         // Gọi LinkGoogleAccountAsync hoặc một phương thức tương tự trong IGoogleUserService
//                         // để cập nhật GoogleId, FullName, ProfilePictureUrl, LoginProvider
//                         bool linkSuccess = await _googleUserService.LinkGoogleAccountAsync(user.Id, googlePayload.Subject, googlePayload.Name, googlePayload.Picture);
//                         if (linkSuccess)
//                         {
//                             // Cập nhật user object local để phản ánh thay đổi đã lưu vào DB
//                             user.GoogleId = googlePayload.Subject;
//                             user.LoginProvider = "Google";
//                             user.FullName = googlePayload.Name; // Hoặc logic phức tạp hơn để quyết định ghi đè
//                             user.ProfilePictureUrl = googlePayload.Picture;
//                             _logger.LogInformation("GoogleSignIn: Successfully linked Google ID {GoogleID} to existing user ID {UserId}", googlePayload.Subject, user.Id);
//                         }
//                         else
//                         {
//                             _logger.LogWarning("GoogleSignIn: Failed to link Google ID {GoogleID} to existing user ID {UserId}. This might be a database issue or concurrent update.", googlePayload.Subject, user.Id);
//                             // Cân nhắc trả về lỗi nếu việc liên kết là bắt buộc và thất bại
//                             return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to link Google account." });
//                         }
//                     }
//                     else if (user.GoogleId != googlePayload.Subject)
//                     {
//                         _logger.LogWarning("GoogleSignIn: Conflict. Email {Email} is already associated with a different Google ID ({ExistingGoogleID}) than the one provided ({ProvidedGoogleID}).", user.Email, user.GoogleId, googlePayload.Subject);
//                         return Conflict(new { message = "This email is already linked to a different Google account in our system." });
//                     }
//                 }
//                 else
//                 {
//                     _logger.LogInformation("GoogleSignIn: User not found by email. Creating new user from Google payload for email: {Email}", googlePayload.Email);
//                     user = await _googleUserService.CreateUserFromGoogleAsync(googlePayload);
//                     _logger.LogInformation("GoogleSignIn: New user created with ID: {UserId}", user.Id);
//                 }
//             }
//             else // User được tìm thấy bằng GoogleId
//             {
//                 _logger.LogInformation("GoogleSignIn: User found by GoogleID: {GoogleID}. Checking for updates.", user.GoogleId);
//                 bool needsDbUpdate = false;
//                 if (user.FullName != googlePayload.Name && !string.IsNullOrEmpty(googlePayload.Name))
//                 {
//                     user.FullName = googlePayload.Name;
//                     needsDbUpdate = true;
//                 }
//                 if (user.ProfilePictureUrl != googlePayload.Picture && !string.IsNullOrEmpty(googlePayload.Picture))
//                 {
//                     user.ProfilePictureUrl = googlePayload.Picture;
//                     needsDbUpdate = true;
//                 }
//                 if (user.Email != googlePayload.Email) // Email Google có thể thay đổi
//                 {
//                     user.Email = googlePayload.Email;
//                     needsDbUpdate = true;
//                 }

//                 if (needsDbUpdate)
//                 {
//                     _logger.LogInformation("GoogleSignIn: Updating user details for UserID: {UserId}", user.Id);
//                     user.UpdatedAt = DateTime.UtcNow;
//                     await _googleUserService.UpdateUserAsync(user); // Cần có UpdateUserAsync trong IGoogleUserService
//                 }
//             }

//             // --- Đăng nhập người dùng bằng Cookie Authentication ---
//             var claims = new List<Claim>
//             {
//                 new Claim(ClaimTypes.NameIdentifier, user.Id), // Quan trọng: User ID của bạn
//                 new Claim(ClaimTypes.Email, user.Email),
//                 new Claim(ClaimTypes.Name, user.FullName ?? string.Empty), // Tên người dùng
//                 new Claim("picture", user.ProfilePictureUrl ?? string.Empty), // URL ảnh đại diện
//                 new Claim(ClaimTypes.Role, user.Role), // Vai trò
//                 // Thêm các claim khác nếu cần, ví dụ:
//                 // new Claim("LoginProvider", user.LoginProvider),
//             };

//             var claimsIdentity = new ClaimsIdentity(
//                 claims, CookieAuthenticationDefaults.AuthenticationScheme); // Chỉ định scheme

//             var authProperties = new AuthenticationProperties
//             {
//                 IsPersistent = true, // Cookie sẽ tồn tại sau khi đóng trình duyệt
//                                      // ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60), // Bạn có thể set thời hạn cụ thể
//                                      // Hoặc để ExpireTimeSpan trong cấu hình cookie xử lý
//                 AllowRefresh = true, // Cho phép làm mới cookie nếu SlidingExpiration = true
//                 // IssuedUtc = DateTimeOffset.UtcNow,
//             };

//             // Thực hiện đăng nhập, tạo cookie session
//             await HttpContext.SignInAsync(
//                 CookieAuthenticationDefaults.AuthenticationScheme,
//                 new ClaimsPrincipal(claimsIdentity),
//                 authProperties);

//             _logger.LogInformation("GoogleSignIn: User {UserId} ({UserEmail}) signed in successfully using cookies.", user.Id, user.Email);

//             var userDto = new GoogleUserDto
//             {
//                 Id = user.Id,
//                 Email = user.Email,
//                 FullName = user.FullName,
//                 ProfilePictureUrl = user.ProfilePictureUrl,
//                 Role = user.Role
//             };

//             // Thay vì trả về token, bạn chỉ cần trả về thông báo thành công và có thể là thông tin người dùng
//             return Ok(new { Message = "Login successful", User = userDto });
//         }

//         [HttpPost("logout")]
//         [Authorize] // Chỉ người dùng đã đăng nhập mới có thể logout
//         public async Task<IActionResult> Logout()
//         {
//             // Xóa cookie xác thực
//             await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
//             _logger.LogInformation("User logged out successfully.");
//             return Ok(new { message = "Logout successful" });
//         }

//         // Endpoint ví dụ để kiểm tra xem user đã đăng nhập chưa
//         [HttpGet("me")]
//         [Authorize] // Yêu cầu xác thực (cookie phải hợp lệ)
//         public IActionResult GetCurrentUser()
//         {
//             // Thông tin người dùng có thể được truy cập từ HttpContext.User
//             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//             var email = User.FindFirstValue(ClaimTypes.Email);
//             var fullName = User.FindFirstValue(ClaimTypes.Name);
//             var picture = User.FindFirstValue("picture");
//             var role = User.FindFirstValue(ClaimTypes.Role);


//             if (string.IsNullOrEmpty(userId))
//             {
//                 return Unauthorized(new { message = "User not authenticated." });
//             }

//             return Ok(new
//             {
//                 Id = userId,
//                 Email = email,
//                 FullName = fullName,
//                 ProfilePictureUrl = picture,
//                 Role = role
//             });
//         }

//         // Endpoint trả về 401 nếu truy cập mà không có cookie hợp lệ (dùng cho cấu hình LoginPath của cookie)
//         [HttpGet("unauthorized")]
//         public IActionResult UnauthorizedPath()
//         {
//             return Unauthorized(new { message = "You are not authorized to access this resource. Please login." });
//         }

//         // Endpoint trả về 403 nếu truy cập mà không có quyền (dùng cho cấu hình AccessDeniedPath của cookie)
//         [HttpGet("forbidden")]
//         public IActionResult ForbiddenPath()
//         {
//             return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have permission to access this resource." });
//         }
//     }
// }