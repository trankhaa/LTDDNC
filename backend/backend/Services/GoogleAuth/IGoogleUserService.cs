// // Services/GoogleUser/IGoogleUserService.cs
// using Google.Apis.Auth;
// using System.Threading.Tasks;
// using backend.Models.Entities;


// namespace backend.Services.GoogleAuth
// {
//     public interface IGoogleUserService
//     {
//         Task<GoogleUser?> GetUserByEmailAsync(string email);
//         Task<GoogleUser?> GetUserByGoogleIdAsync(string googleId);
//         Task<GoogleUser> CreateUserFromGoogleAsync(GoogleJsonWebSignature.Payload googlePayload);
//         Task<bool> LinkGoogleAccountAsync(string userId, string googleId, string? fullName, string? profilePictureUrl);
//         Task<bool> UpdateUserAsync(GoogleUser userToUpdate);
//         Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string authorizationCode);

//     }
// }
