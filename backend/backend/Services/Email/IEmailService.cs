// File: backend/Services/IEmailService.cs

using System;
using System.Threading.Tasks;


// File: backend/Services/IEmailService.cs
namespace backend.Services
{
    public interface IEmailService
    {
        Task SendBookingConfirmationEmailAsync(string toEmail, string patientName, string doctorName, DateTime appointmentTime, string location, string? note, decimal price);
        Task SendPaymentRequestEmailAsync(string toEmail, string patientName, decimal amount, string doctorName, DateTime appointmentTime, string location, string paymentLink);
    }
}
