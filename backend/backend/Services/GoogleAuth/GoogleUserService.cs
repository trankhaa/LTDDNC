// using Google.Apis.Auth;
// using Microsoft.Extensions.Options;
// using MongoDB.Driver;
// using System;
// using System.Threading.Tasks;
// using backend.Models.Entities; // <- Đảm bảo bạn thêm namespace đúng chỗ chứa GoogleUser.cs
// using backend.Settings; // ← THÊM: để tìm thấy MongoDbSettings
// using Google.Apis.Auth;
// using Google.Apis.Auth.OAuth2;
// using Google.Apis.Auth.OAuth2.Flows;
// using Google.Apis.Auth.OAuth2.Responses;


// namespace backend.Services.GoogleAuth
// {
//     public class GoogleUserService : IGoogleUserService
//     {
//         private readonly IMongoCollection<GoogleUser> _usersCollection;
//         private readonly GoogleAuthSettings _googleAuthSettings;
//         public GoogleUserService(IOptions<MongoDbSettings> mongoDbSettings)
//         {
//             var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
//             var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
//             _usersCollection = mongoDatabase.GetCollection<GoogleUser>(
//                 mongoDbSettings.Value.Collections.UsersGoogleCollectionName
//             );

//         }

//         public async Task<GoogleUser?> GetUserByEmailAsync(string email)
//         {
//             return await _usersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
//         }

//         public async Task<GoogleUser?> GetUserByGoogleIdAsync(string googleId)
//         {
//             return await _usersCollection.Find(u => u.GoogleId == googleId).FirstOrDefaultAsync();
//         }

//         public async Task UpdateUserGoogleIdAsync(string userId, string googleId)
//         {
//             var filter = Builders<GoogleUser>.Filter.Eq(u => u.Id, userId);
//             var update = Builders<GoogleUser>.Update
//                 .Set(u => u.GoogleId, googleId)
//                 .Set(u => u.UpdatedAt, DateTime.UtcNow);

//             await _usersCollection.UpdateOneAsync(filter, update);
//         }

//         public async Task<GoogleUser> CreateUserFromGoogleAsync(GoogleJsonWebSignature.Payload googlePayload)
//         {
//             var newUser = new GoogleUser
//             {
//                 Email = googlePayload.Email,
//                 FullName = googlePayload.Name,
//                 GoogleId = googlePayload.Subject,
//                 ProfilePictureUrl = googlePayload.Picture,
//                 IsActive = true,
//                 Role = "Patient",
//                 LoginProvider = "Google",
//                 Password = null,
//                 CreatedAt = DateTime.UtcNow,
//                 UpdatedAt = DateTime.UtcNow
//             };

//             await _usersCollection.InsertOneAsync(newUser);
//             return newUser;
//         }



//         public async Task<bool> LinkGoogleAccountAsync(string userId, string googleId, string? fullName, string? profilePictureUrl)
//         {
//             var filter = Builders<GoogleUser>.Filter.Eq(u => u.Id, userId);
//             var updateDefinition = Builders<GoogleUser>.Update
//                 .Set(u => u.GoogleId, googleId)
//                 .Set(u => u.LoginProvider, "Google")
//                 .Set(u => u.UpdatedAt, DateTime.UtcNow);

//             if (!string.IsNullOrEmpty(fullName))
//             {
//                 updateDefinition = updateDefinition.Set(u => u.FullName, fullName);
//             }
//             if (!string.IsNullOrEmpty(profilePictureUrl))
//             {
//                 updateDefinition = updateDefinition.Set(u => u.ProfilePictureUrl, profilePictureUrl);
//             }

//             var result = await _usersCollection.UpdateOneAsync(filter, updateDefinition);
//             return result.IsAcknowledged && result.ModifiedCount > 0;
//         }

//         public async Task<bool> UpdateUserAsync(GoogleUser userToUpdate)
//         {
//             var filter = Builders<GoogleUser>.Filter.Eq(u => u.Id, userToUpdate.Id);
//             var updateDefinition = Builders<GoogleUser>.Update
//                 .Set(u => u.FullName, userToUpdate.FullName)
//                 .Set(u => u.ProfilePictureUrl, userToUpdate.ProfilePictureUrl)
//                 .Set(u => u.Email, userToUpdate.Email)
//                 .Set(u => u.GoogleId, userToUpdate.GoogleId) // Nếu bạn cho phép cập nhật GoogleId qua hàm này
//                 .Set(u => u.LoginProvider, userToUpdate.LoginProvider)
//                 .Set(u => u.Role, userToUpdate.Role)
//                 .Set(u => u.IsActive, userToUpdate.IsActive)
//                 .Set(u => u.UpdatedAt, DateTime.UtcNow); // Luôn cập nhật UpdatedAt

//             var result = await _usersCollection.UpdateOneAsync(filter, updateDefinition);
//             return result.IsAcknowledged && result.ModifiedCount > 0;
//         }

//         // Đảm bảo UpdateUserGoogleIdAsync cũng có trong class nếu bạn khai báo nó trong interface
//         // public async Task UpdateUserGoogleIdAsync(string userId, string googleId)
//         // {
//         //     var filter = Builders<GoogleUser>.Filter.Eq(u => u.Id, userId);
//         //     var update = Builders<GoogleUser>.Update
//         //         .Set(u => u.GoogleId, googleId)
//         //         // Cân nhắc cập nhật thêm LoginProvider và các thông tin khác ở đây nếu cần
//         //         // .Set(u => u.LoginProvider, "Google")
//         //         .Set(u => u.UpdatedAt, DateTime.UtcNow);
//         //     await _usersCollection.UpdateOneAsync(filter, update);
//         // }




//         // GoogleAuthService.cs
//         // GoogleAuthService.cs
//         public async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string authorizationCode)
//         {
//             try
//             {
//                 var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
//                 {
//                     ClientSecrets = new ClientSecrets
//                     {
//                         ClientId = _googleAuthSettings.ClientId,
//                         ClientSecret = _googleAuthSettings.ClientSecret
//                     },
//                 });

//                 var tokenResponse = await flow.ExchangeCodeForTokenAsync(
//                     userId: "me", // Có thể là một chuỗi bất kỳ, "me" là phổ biến.
//                     code: authorizationCode,
//                     // 🚨 SỬA CHỖ NÀY: Dùng RedirectUri từ cấu hình của bạn
//                     // RedirectUri này PHẢI KHỚP CHÍNH XÁC với một trong các "Authorized redirect URIs"
//                     // đã cấu hình trong Google Cloud Console cho OAuth client này.
//                     // "postmessage" dành cho luồng JavaScript phía client, không phải cho việc đổi code ở server.
//                     redirectUri: _googleAuthSettings.RedirectUri, //  <-- THAY ĐỔI DÒNG NÀY
//                     CancellationToken.None // Bạn đã xóa tham số này do lỗi CS1739, đó là đúng.
//                 );

//                 string idToken = tokenResponse.IdToken;

//                 var validationSettings = new GoogleJsonWebSignature.ValidationSettings
//                 {
//                     Audience = new[] { _googleAuthSettings.ClientId } // Audience phải là Client ID của backend
//                 };

//                 var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
//                 return payload;
//             }
//             // Trong GoogleAuthService.cs, bên trong phương thức VerifyGoogleTokenAsync
//             catch (TokenResponseException tex)
//             {
//                 // Sử dụng các thuộc tính đúng của tex.Error
//                 Console.WriteLine($"Google TokenResponseException: Error: {tex.Error.Error}, Description: {tex.Error.ErrorDescription}, URI: {tex.Error.ErrorUri}");
//                 Console.WriteLine($"Full exception: {tex.ToString()}");
//                 return null;
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Google token verification failed (generic): {ex.Message}");
//                 Console.WriteLine($"Full exception: {ex.ToString()}"); // Ghi log toàn bộ exception để có thêm chi tiết
//                 return null;
//             }
//         }
//     }

// }
