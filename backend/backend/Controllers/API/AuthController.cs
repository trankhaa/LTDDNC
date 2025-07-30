// // Controllers/AuthController.cs
// using backend.Models.Entities;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Options; // Để inject IOptions
// using System.Threading.Tasks; // Để dùng Task
// using backend.Services.User; // Giả sử các service đã được định nghĩa và đăng ký

// // Giả sử các service đã được định nghĩa và đăng ký
// // using backend.Services; 

// public class GoogleLoginRequestDto
// {
//     public string AuthorizationCode { get; set; } = string.Empty;
// }

// [ApiController]
// [Route("api/[controller]")]
// public class AuthController : ControllerBase
// {
//     private readonly IGoogleAuthService _googleAuthService;
//     private readonly IUserService _userService;
//     private readonly ITokenService _tokenService;

//     public AuthController(
//         IGoogleAuthService googleAuthService,
//         IUserService userService,
//         ITokenService tokenService)
//     {
//         _googleAuthService = googleAuthService;
//         _userService = userService;
//         _tokenService = tokenService;
//     }

//     [HttpPost("google-signin")]
//     public async Task<IActionResult> GoogleSignIn([FromBody] GoogleLoginRequestDto request)
//     {
//         if (string.IsNullOrEmpty(request.AuthorizationCode))
//         {
//             return BadRequest("Authorization code is missing.");
//         }

//         var googlePayload = await _googleAuthService.VerifyGoogleTokenAsync(request.AuthorizationCode);

//         if (googlePayload == null)
//         {
//             return BadRequest("Invalid Google token or code.");
//         }

//         // Kiểm tra xem user có tồn tại bằng GoogleId không
//         var user = await _userService.GetUserByGoogleIdAsync(googlePayload.Subject);

//         if (user == null) // Nếu không tìm thấy bằng GoogleId, thử tìm bằng Email
//         {
//             user = await _userService.GetUserByEmailAsync(googlePayload.Email);
//             if (user != null)
//             {
//                 // User tồn tại với email này nhưng chưa liên kết GoogleId
//                 // Cập nhật GoogleId cho user hiện tại
//                 if (string.IsNullOrEmpty(user.GoogleId))
//                 {
//                     await _userService.UpdateUserGoogleIdAsync(user.Id, googlePayload.Subject);
//                     user.GoogleId = googlePayload.Subject; // Cập nhật object user local
//                     user.LoginProvider = "Google"; // Có thể cập nhật cả login provider
//                 }
//                 else if (user.GoogleId != googlePayload.Subject)
//                 {
//                     // Email đã được dùng bởi một tài khoản Google khác hoặc tài khoản local
//                     // Xử lý tùy theo logic nghiệp vụ (ví dụ: báo lỗi)
//                     return Conflict("This email is already associated with another account.");
//                 }
//             }
//             else
//             {
//                 // User hoàn toàn mới, tạo user từ thông tin Google
//                 user = await _userService.CreateUserFromGoogleAsync(googlePayload);
//             }
//         }

//         // Tại thời điểm này, 'user' đã được tìm thấy hoặc tạo mới
//         // Tạo JWT token cho user
//         var appToken = _tokenService.GenerateJwtToken(user);

//         return Ok(new
//         {
//             Token = appToken,
//             User = new // Trả về thông tin user cơ bản nếu cần
//             {
//                 user.Id,
//                 user.Email,
//                 user.FullName,
//                 user.Role,
//                 user.ProfilePictureUrl
//             }
//         });
//     }
// }