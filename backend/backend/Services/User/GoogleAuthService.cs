// // Services/GoogleAuthService.cs
// using Google.Apis.Auth;
// using Microsoft.Extensions.Options;
// using System.Threading.Tasks;
// using backend.Models.Entities;
// public class GoogleAuthSettings
// {
//     public string ClientId { get; set; } = string.Empty;
//     public string ClientSecret { get; set; } = string.Empty;
//     public string RedirectUri { get; set; } = string.Empty; // Backend's redirect URI for token exchange
// }

// public interface IGoogleAuthService
// {
//     Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string authorizationCode);
// }

// public class GoogleAuthService : IGoogleAuthService
// {
//     private readonly GoogleAuthSettings _googleAuthSettings;

//     public GoogleAuthService(IOptions<GoogleAuthSettings> googleAuthSettings)
//     {
//         _googleAuthSettings = googleAuthSettings.Value;
//     }

//     public async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string authorizationCode)
//     {
//         try
//         {
//             var tokenResponse = await GoogleAuthorizationCodeFlow.Initializer;
//             {
//                 ClientSecrets = new ClientSecrets
//                 {
//                     ClientId = _googleAuthSettings.ClientId,
//                     ClientSecret = _googleAuthSettings.ClientSecret
//                 };
//                 // Scopes có thể không cần thiết ở đây nếu id_token đã đủ thông tin
//                 // Scopes = new[] { "email", "profile" } 
//             }
//             ExchangeCodeForTokenAsync(
//                 userId: "user", // "user" là một placeholder, không quan trọng trong flow này
//                 code: authorizationCode,
//                 // Redirect URI này phải khớp với một trong các Authorized redirect URIs
//                 // đã đăng ký trong Google Cloud Console và dùng cho backend.
//                 redirectUri: _googleAuthSettings.RedirectUri,
//                 cancellationToken: CancellationToken.None
//             );

//             // idToken chứa thông tin người dùng được ký bởi Google
//             string idToken = tokenResponse.IdToken;

//             // Xác thực idToken
//             // Settings này đảm bảo token được cấp cho Client ID của bạn
//             var validationSettings = new GoogleJsonWebSignature.ValidationSettings
//             {
//                 Audience = new[] { _googleAuthSettings.ClientId }
//             };

//             // ValidateAsync sẽ throw exception nếu token không hợp lệ
//             GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
//             return payload;
//         }
//         catch (Exception ex)
//         {
//             // Log lỗi
//             Console.WriteLine($"Google token verification failed: {ex.Message}");
//             return null;
//         }
//     }



//     // Services/UserService.cs (Một phần ví dụ)
//     public interface IGoogleUserService
//     {
//         Task<User?> GetUserByEmailAsync(string email);
//         Task<User?> GetUserByGoogleIdAsync(string googleId);
//         Task<User> CreateUserFromGoogleAsync(GoogleJsonWebSignature.Payload googlePayload);
//         Task UpdateUserGoogleIdAsync(string userId, string googleId);
//     }

//     public class GoogleUserService : IGoogleUserService // Triển khai chi tiết với MongoDB Driver
//     {
//         private readonly IMongoCollection<User> _usersCollection;

//         public UserService(IOptions<MongoDBSettings> mongoDBSettings) // Giả sử bạn có MongoDBSettings
//         {
//             var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionString);
//             var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
//             _usersCollection = mongoDatabase.GetCollection<User>(mongoDBSettings.Value.UsersCollectionName);
//         }

//         public async Task<User?> GetUserByEmailAsync(string email)
//         {
//             return await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
//         }

//         public async Task<User?> GetUserByGoogleIdAsync(string googleId)
//         {
//             return await _usersCollection.Find(u => u.GoogleId == googleId).FirstOrDefaultAsync();
//         }

//         public async Task UpdateUserGoogleIdAsync(string userId, string googleId)
//         {
//             var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
//             var update = Builders<User>.Update.Set(u => u.GoogleId, googleId)
//                                              .Set(u => u.UpdatedAt, DateTime.UtcNow);
//             await _usersCollection.UpdateOneAsync(filter, update);
//         }

//         public async Task<User> CreateUserFromGoogleAsync(GoogleJsonWebSignature.Payload googlePayload)
//         {
//             var newUser = new User
//             {
//                 Email = googlePayload.Email,
//                 FullName = googlePayload.Name,
//                 GoogleId = googlePayload.Subject, // "sub" là Google User ID
//                 ProfilePictureUrl = googlePayload.Picture,
//                 IsActive = true, // Hoặc googlePayload.EmailVerified
//                 Role = "Patient", // Hoặc vai trò mặc định khác
//                 LoginProvider = "Google",
//                 Password = null, // Không có mật khẩu cho Google login
//                 CreatedAt = DateTime.UtcNow,
//                 UpdatedAt = DateTime.UtcNow
//             };
//             await _usersCollection.InsertOneAsync(newUser);
//             return newUser;
//         }
//     }
// }
