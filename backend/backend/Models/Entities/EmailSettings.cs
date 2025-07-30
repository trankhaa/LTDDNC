namespace backend.Models.Entities
{
    public class EmailSettings
    {
        public string SenderName { get; set; } = string.Empty;     // Ví dụ: HealthCare Booking
        public string SenderEmail { get; set; } = string.Empty;    // Ví dụ: onboarding@resend.dev
        public string Password { get; set; } = string.Empty;       // Resend API key
    }
}
