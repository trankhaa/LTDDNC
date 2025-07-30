// File: backend/Models/Configs/PayOSSettings.cs
namespace backend.Models
{
    public class PayOSSettings
    {
        public string? ClientId { get; set; }
        public string? ApiKey { get; set; }
        public string? ChecksumKey { get; set; }
    }
}