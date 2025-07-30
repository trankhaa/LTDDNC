namespace backend.Helper
{
    public class BcryptHelper : IBcryptHelper
    {
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password), "Mật khẩu không được để trống.");
            }
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            {
                return false;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Xử lý lỗi nếu chuỗi hash không hợp lệ (ví dụ: không chứa salt hợp lệ)
                // Ghi log lỗi ở đây nếu cần
                return false;
            }
        }
    }
}

