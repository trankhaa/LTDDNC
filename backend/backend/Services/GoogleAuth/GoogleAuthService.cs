// // File: backend/Services/GoogleAuth/GoogleAuthService.cs
// using Google.Apis.Auth;
// using Google.Apis.Auth.OAuth2;
// using Google.Apis.Auth.OAuth2.Flows;
// using Google.Apis.Auth.OAuth2.Responses;
// using Microsoft.Extensions.Logging; // THÊM USING NÀY
// using Microsoft.Extensions.Options;
// using System;
// using System.Linq; // THÊM USING NÀY
// using System.Threading;
// using System.Threading.Tasks;

// namespace backend.Services.GoogleAuth
// {
//     public class GoogleAuthSettings
//     {
//         public string ClientId { get; set; } = string.Empty;
//         public string ClientSecret { get; set; } = string.Empty;
//         public string RedirectUri { get; set; } = string.Empty;
//     }

//     public interface IGoogleAuthService
//     {
//         Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string authorizationCode);
//     }

//     public class GoogleAuthService : IGoogleAuthService
//     {
//         private readonly GoogleAuthSettings _googleAuthSettings;
//         private readonly ILogger<GoogleAuthService> _logger; // Logger

//         public GoogleAuthService(IOptions<GoogleAuthSettings> googleAuthSettings, ILogger<GoogleAuthService> logger) // Inject Logger
//         {
//             _googleAuthSettings = googleAuthSettings.Value;
//             _logger = logger; // Khởi tạo logger
//         }

//         public async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string authorizationCode)
//         {
//             _logger.LogInformation("Attempting to verify Google token with ClientID: {ClientId} and RedirectUri: {RedirectUri} for code: {AuthCode}",
//                                  _googleAuthSettings.ClientId, _googleAuthSettings.RedirectUri, authorizationCode);

//             if (string.IsNullOrEmpty(_googleAuthSettings.ClientId) ||
//                 string.IsNullOrEmpty(_googleAuthSettings.ClientSecret) ||
//                 string.IsNullOrEmpty(_googleAuthSettings.RedirectUri))
//             {
//                 _logger.LogError("GoogleAuthSettings are not configured properly. ClientId, ClientSecret, or RedirectUri is missing.");
//                 return null;
//             }
//             if (string.IsNullOrEmpty(authorizationCode))
//             {
//                 _logger.LogWarning("Authorization code is null or empty.");
//                 return null;
//             }

//             try
//             {
//                 var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
//                 {
//                     ClientSecrets = new ClientSecrets
//                     {
//                         ClientId = _googleAuthSettings.ClientId,
//                         ClientSecret = _googleAuthSettings.ClientSecret
//                     }
//                 });

//                 TokenResponse tokenResponse = await flow.ExchangeCodeForTokenAsync(
//                     userId: "user",
//                     code: authorizationCode,
//                     redirectUri: _googleAuthSettings.RedirectUri, // <-- SỬ DỤNG RedirectUri TỪ CẤU HÌNH
//                     CancellationToken.None
//                 ).ConfigureAwait(false);

//                 _logger.LogInformation("Token exchange successful. Received ID token.");
//                 // _logger.LogDebug("ID Token: {IdToken}", tokenResponse.IdToken); // Bật khi debug

//                 string idToken = tokenResponse.IdToken;

//                 var validationSettings = new GoogleJsonWebSignature.ValidationSettings
//                 {
//                     Audience = new[] { _googleAuthSettings.ClientId }
//                 };

//                 _logger.LogInformation("Validating ID token with audience: {Audience}", validationSettings.Audience.FirstOrDefault());
//                 var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings).ConfigureAwait(false);
//                 _logger.LogInformation("ID token validation successful. User: {Email}", payload.Email);
//                 return payload;
//             }
//             catch (TokenResponseException tex)
//             {
//                 _logger.LogError(tex, "Google TokenResponseException during code exchange. Error: {Error}, Description: {Description}, Uri: {Uri}. Check ClientId, ClientSecret, and RedirectUri in appsettings.json and Google Cloud Console.",
//                     tex.Error?.Error, tex.Error?.ErrorDescription, tex.Error?.ErrorUri); // Thêm ? để tránh null ref nếu tex.Error là null
//                 return null;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Generic exception during Google token verification.");
//                 return null;
//             }
//         }
//     }
// }