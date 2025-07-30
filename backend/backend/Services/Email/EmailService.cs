// Đảm bảo namespace là "backend.Services"
namespace backend.Services;

using Resend; // Dùng using của gói chính thức
using System;
using System.Threading.Tasks;

// Class này triển khai interface IEmailService
public class EmailService : IEmailService
{
    private readonly IResend _resend;

    // Inject IResend đã được đăng ký trong Program.cs
    public EmailService(IResend resend)
    {
        _resend = resend;
    }

    // Triển khai phương thức xác nhận đặt lịch
    public async Task SendBookingConfirmationEmailAsync(string toEmail, string patientName, string doctorName, DateTime appointmentTime, string location, string? note, decimal price)
    {
        var subject = "Xác nhận đặt lịch khám thành công";
        var htmlBody = $@"
            <!DOCTYPE html>
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                <h2>Xác nhận đặt lịch khám thành công</h2>
                <p>Xin chào <strong>{patientName}</strong>,</p>
                <p>Lịch hẹn của bạn đã được xác nhận thành công. Dưới đây là thông tin chi tiết:</p>
                <ul style='list-style-type: none; padding: 0;'>
                    <li style='margin-bottom: 10px;'><strong>Bác sĩ:</strong> {doctorName}</li>
                    <li style='margin-bottom: 10px;'><strong>Thời gian:</strong> {appointmentTime:HH:mm} ngày {appointmentTime:dd/MM/yyyy}</li>
                    <li style='margin-bottom: 10px;'><strong>Địa điểm/Phòng khám:</strong> {location}</li>
                    {(string.IsNullOrEmpty(note) ? "" : $"<li style='margin-bottom: 10px;'><strong>Ghi chú của bạn:</strong> {note}</li>")}
                    <li style='margin-bottom: 10px;'><strong>Chi phí khám (dự kiến):</strong> {price:N0} VND</li>
                </ul>
                <p>Vui lòng đến đúng giờ để buổi khám được diễn ra thuận lợi. Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
                <hr>
                <p style='font-size: 0.8em; color: #888;'>Đây là email tự động, vui lòng không trả lời.</p>
            </body>
            </html>";

        // Tạo đối tượng EmailMessage theo đúng cú pháp của thư viện Resend
        var message = new EmailMessage
        {
            From = "Acme <onboarding@resend.dev>", // Quan trọng: Thay bằng domain đã verify của bạn
            To = { toEmail }, // ✅ SỬA LỖI: Gán trực tiếp, không dùng new List<string>
            Subject = subject,
            HtmlBody = htmlBody
        };

        // Gửi email
        await _resend.EmailSendAsync(message);
    }

    // Triển khai phương thức yêu cầu thanh toán
    public async Task SendPaymentRequestEmailAsync(string toEmail, string patientName, decimal amount, string doctorName, DateTime appointmentTime, string location, string paymentLink)
    {
        var subject = "Yêu cầu thanh toán chi phí khám bệnh";
        var htmlBody = $@"
            <!DOCTYPE html>
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                <h2>Yêu cầu thanh toán chi phí khám bệnh</h2>
                <p>Xin chào <strong>{patientName}</strong>,</p>
                <p>Chúng tôi gửi đến bạn yêu cầu thanh toán cho lịch hẹn khám với bác sĩ <strong>{doctorName}</strong> vào lúc <strong>{appointmentTime:HH:mm} ngày {appointmentTime:dd/MM/yyyy}</strong> tại <strong>{location}</strong>.</p>
                <p>Số tiền cần thanh toán: <strong>{amount:N0} VND</strong></p>
                <p style='text-align: center; margin: 20px 0;'>
                    <a href='{paymentLink}' style='background-color: #007bff; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-size: 16px;'>Thanh toán ngay</a>
                </p>
                <p>Nếu bạn có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi.</p>
                <hr>
                <p style='font-size: 0.8em; color: #888;'>Đây là email tự động, vui lòng không trả lời.</p>
            </body>
            </html>";
        
        // Tạo đối tượng EmailMessage
        var message = new EmailMessage
        {
            From = "Acme <onboarding@resend.dev>", // Quan trọng: Thay bằng domain đã verify của bạn
            To = { toEmail }, // ✅ SỬA LỖI: Gán trực tiếp, không dùng new List<string>
            Subject = subject,
            HtmlBody = htmlBody
        };

        // Gửi email
        await _resend.EmailSendAsync(message);
    }
}