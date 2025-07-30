// File: backend/Controllers/API/BookingController/PaymentController.cs

using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.Models.Entities.Booking;
using System.Threading.Tasks;
using Net.payOS;
using Net.payOS.Types; // Quan trọng: using đúng namespace của thư viện

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly PayOS _payOS;
    private readonly ConfirmAppointmentService _confirmAppointmentService;

    public PaymentController(PayOS payOS, ConfirmAppointmentService confirmAppointmentService)
    {
        _payOS = payOS;
        _confirmAppointmentService = confirmAppointmentService;
    }

    [HttpPost("payos-webhook-handler")]
    public async Task<IActionResult> HandleWebhook([FromBody] WebhookType webhookBody)
    {
        Console.WriteLine($"🔔 WEBHOOK RECEIVED at {DateTime.UtcNow}");
        Console.WriteLine($"📦 Webhook Data: {System.Text.Json.JsonSerializer.Serialize(webhookBody)}");

        try
        {
            // Xác thực webhook data
            WebhookData verifiedWebhookData = _payOS.verifyPaymentWebhookData(webhookBody);
            Console.WriteLine($"✅ Webhook verified for OrderCode: {verifiedWebhookData.orderCode}");

            // Lấy thông tin chi tiết từ PayOS
            PaymentLinkInformation paymentInfo = await _payOS.getPaymentLinkInformation(verifiedWebhookData.orderCode);
            Console.WriteLine($"💰 Payment Status from PayOS: {paymentInfo.status}");

            if (paymentInfo.status == "PAID")
            {
                Console.WriteLine($"🔄 Updating appointment status for OrderCode: {paymentInfo.orderCode}");

                bool updated = await _confirmAppointmentService.UpdateAppointmentStatusByOrderCodeAsync(
                    paymentInfo.orderCode,
                    PaymentStatus.PAID
                );

                if (updated)
                {
                    Console.WriteLine($"✅ WEBHOOK SUCCESS: Order {paymentInfo.orderCode} has been updated to PAID.");
                }
                else
                {
                    Console.WriteLine($"⚠️ WEBHOOK WARNING: Order {paymentInfo.orderCode} was not updated (maybe already paid or not found).");
                }
            }
            else
            {
                Console.WriteLine($"ℹ️ WEBHOOK INFO: Order {paymentInfo.orderCode} has status {paymentInfo.status}, no action needed.");
            }

            return Ok(new { error = 0, message = "Success" });
        }
        catch (Exception e)
        {
            Console.WriteLine($"❌ WEBHOOK ERROR: {e.Message}");
            Console.WriteLine($"🔍 Stack Trace: {e.StackTrace}");

            // Vẫn trả về success để PayOS không retry
            return Ok(new { error = -1, message = "Webhook processing failed", data = e.Message });
        }
    }

    [HttpPost("manual-update-status/{orderCode}")]
    public async Task<IActionResult> ManualUpdatePaymentStatus(long orderCode, [FromQuery] string status = "PAID")
    {
        try
        {
            // Kiểm tra trạng thái thực tế từ PayOS
            PaymentLinkInformation paymentInfo = await _payOS.getPaymentLinkInformation(orderCode);

            Console.WriteLine($"🔍 PayOS Status for Order {orderCode}: {paymentInfo.status}");

            if (paymentInfo.status == "PAID")
            {
                // Cập nhật database
                bool updated = await _confirmAppointmentService.UpdateAppointmentStatusByOrderCodeAsync(orderCode, PaymentStatus.PAID);

                if (updated)
                {
                    return Ok(new
                    {
                        message = "Đã cập nhật trạng thái thành công",
                        orderCode = orderCode,
                        newStatus = PaymentStatus.PAID,
                        payOsStatus = paymentInfo.status
                    });
                }
                else
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng hoặc trạng thái đã được cập nhật" });
                }
            }
            else
            {
                return BadRequest(new
                {
                    message = "Đơn hàng chưa được thanh toán trên PayOS",
                    currentStatus = paymentInfo.status
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
        }
    }

    [HttpGet("check-payos-status/{orderCode}")]
    public async Task<IActionResult> CheckPayOSStatus(long orderCode)
    {
        try
        {
            PaymentLinkInformation paymentInfo = await _payOS.getPaymentLinkInformation(orderCode);

            return Ok(new
            {
                orderCode = orderCode,
                payOsStatus = paymentInfo.status,
                amount = paymentInfo.amount,
                // ✅ FIXED: Xóa thuộc tính 'description' không tồn tại
                createdAt = paymentInfo.createdAt,
                transactions = paymentInfo.transactions
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Không thể kiểm tra trạng thái từ PayOS", error = ex.Message });
        }
    }
}