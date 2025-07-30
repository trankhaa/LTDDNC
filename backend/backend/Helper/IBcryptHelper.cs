namespace backend.Helper
{
    public interface IBcryptHelper
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}