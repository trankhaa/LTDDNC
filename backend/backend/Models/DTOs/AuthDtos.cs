namespace backend.Models.DTOs // Hoặc namespace bạn muốn
{
    public class GoogleSignInRequest
    {
        public string AuthorizationCode { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public GoogleUserDto User { get; set; } = new GoogleUserDto();
    }

    public class GoogleUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}