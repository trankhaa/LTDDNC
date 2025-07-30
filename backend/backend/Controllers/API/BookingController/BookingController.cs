using Microsoft.AspNetCore.Mvc;
using backend.Models.Entities.Booking;
using backend.Services;
using System.Threading.Tasks;
using Net.payOS;
using Net.payOS.Types;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly ConfirmAppointmentService _confirmAppointmentService;
        private readonly PayOS _payOS;
        private readonly IEmailService _emailService; // ✅ FIELD ĐÃ KHAI BÁO

        // ✅ SỬA LẠI CONSTRUCTOR - THÊM IEmailService VÀO THAM SỐ
        public BookingController(ConfirmAppointmentService confirmAppointmentService, PayOS payOS, IEmailService emailService)
        {
            _confirmAppointmentService = confirmAppointmentService;
            _payOS = payOS;
            _emailService = emailService; // ✅ GÁN VÀO FIELD
        }

        [HttpGet("check-slot")]
        public async Task<IActionResult> CheckSlotTaken([FromQuery] string doctorId, [FromQuery] DateTime date, [FromQuery] string slot)
        {
            if (string.IsNullOrEmpty(doctorId) || string.IsNullOrEmpty(slot))
            {
                return BadRequest("DoctorId và slot không được để trống.");
            }

            bool isTaken = await _confirmAppointmentService.IsSlotTakenAsync(doctorId, date, slot);

            return Ok(new { isTaken });
        }

        [HttpGet("appointments/doctor/{doctorId}")]
        public async Task<IActionResult> GetAllAppointmentsByDoctorId(string doctorId)
        {
            var appointments = await _confirmAppointmentService.GetAppointmentsByDoctorIdAsync(doctorId);

            if (appointments == null || !appointments.Any())
            {
                return NotFound($"Không tìm thấy lịch hẹn nào cho bác sĩ có ID: {doctorId}");
            }

            return Ok(appointments);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAppointment([FromBody] ConfirmAppointment appointment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra slot đã được đặt chưa
            bool isTaken = await _confirmAppointmentService.IsSlotTakenAsync(
                appointment.DoctorId,
                appointment.Date,
                appointment.Slot
            );

            if (isTaken)
            {
                return Conflict("Slot đã được đặt. Vui lòng chọn thời gian khác.");
            }

            await _confirmAppointmentService.CreateAppointmentAsync(appointment);

            return Ok(new
            {
                message = "Đặt lịch thành công!",
                appointment
            });
        }

        [HttpGet("doctor/{doctorId}/date/{date}")]
        public async Task<IActionResult> GetAppointmentsByDoctorAndDate(string doctorId, DateTime date)
        {
            var result = await _confirmAppointmentService.GetAppointmentsByDoctorAndDateAsync(doctorId, date);
            return Ok(result);
        }

        [HttpGet("check-patient-booking")]
        public async Task<IActionResult> CheckPatientBooking([FromQuery] string patientId, [FromQuery] string doctorId, [FromQuery] DateTime date, [FromQuery] string slot)
        {
            if (string.IsNullOrEmpty(patientId) || string.IsNullOrEmpty(doctorId) || string.IsNullOrEmpty(slot))
            {
                return BadRequest("PatientId, DoctorId và slot không được để trống.");
            }

            bool hasBooking = await _confirmAppointmentService.CheckPatientBookingAsync(patientId, doctorId, date, slot);

            return Ok(new { hasBooking });
        }

        [HttpGet("check-daily-limit")]
        public async Task<IActionResult> CheckDailyBookingLimit(
           [FromQuery] string patientId,
           [FromQuery] string doctorId,
           [FromQuery] DateTime date)
        {
            if (string.IsNullOrEmpty(patientId) || string.IsNullOrEmpty(doctorId))
            {
                return BadRequest("PatientId và DoctorId không được để trống.");
            }

            bool hasBookingToday = await _confirmAppointmentService.HasPatientBookedWithDoctorOnDateAsync(patientId, doctorId, date);

            return Ok(new { hasBookingToday });
        }

        [HttpPost("create-and-get-payment-link")]
        public async Task<IActionResult> CreateAppointmentAndGetPaymentLink([FromBody] ConfirmAppointment appointment)
        {
            // === BƯỚC 1: KIỂM TRA DỮ LIỆU ĐẦU VÀO ===
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // === BƯỚC 2: KIỂM TRA LOGIC NGHIỆP VỤ (SLOT ĐÃ ĐƯỢC ĐẶT CHƯA) ===
                bool isTaken = await _confirmAppointmentService.IsSlotTakenAsync(appointment.DoctorId, appointment.Date, appointment.Slot);
                if (isTaken)
                {
                    return Conflict(new { message = "Slot đã được đặt. Vui lòng chọn thời gian khác." });
                }

                // === BƯỚC 3: CHUẨN BỊ VÀ LƯU LỊCH HẸN VÀO DATABASE ===
                appointment.OrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                appointment.Status = PaymentStatus.PENDING_PAYMENT;

                await _confirmAppointmentService.CreateAppointmentAsync(appointment);
                Console.WriteLine($"[INFO] Đã tạo lịch hẹn trong DB với OrderCode: {appointment.OrderCode}");

                // === BƯỚC 4: TẠO LINK THANH TOÁN VỚI PAYOS ===
                var items = new List<ItemData>
                {
                    new ItemData(
                        name: $"Phí khám {appointment.NameDr}",
                        quantity: 1,
                        price: (int)appointment.ConsultationFee
                    )
                };

                var paymentData = new PaymentData(
                    orderCode: appointment.OrderCode,
                    amount: (int)appointment.ConsultationFee,
                    description: $"TT lich hen {appointment.OrderCode}",
                    items: items,
                    returnUrl: $"http://localhost:5173",
                    cancelUrl: $"http://localhost:5173/payment-result"
                );

                CreatePaymentResult paymentResult = await _payOS.createPaymentLink(paymentData);
                Console.WriteLine($"[INFO] Đã tạo link thanh toán PayOS cho OrderCode: {appointment.OrderCode}");

                // === BƯỚC 5: GỬI EMAIL YÊU CẦU THANH TOÁN (KÈM LINK) ===
                try
                {
                    await _emailService.SendPaymentRequestEmailAsync(
    toEmail: appointment.PatientEmail,
    patientName: appointment.PatientName,
    amount: appointment.ConsultationFee, // ✅ Đã sửa đúng
    doctorName: appointment.NameDr,
    appointmentTime: appointment.Date,      // <--- KIỂM TRA DÒNG NÀY
    location: appointment.Slot,             // <--- KIỂM TRA DÒNG NÀY
    paymentLink: paymentResult.checkoutUrl  // <--- KIỂM TRA DÒNG NÀY
);
                    Console.WriteLine($"[INFO] Đã gửi email yêu cầu thanh toán cho OrderCode: {appointment.OrderCode}");
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"[WARNING] Gửi email yêu cầu thanh toán thất bại cho OrderCode {appointment.OrderCode}. Lỗi: {emailEx.Message}");
                }

                // === BƯỚC 6: TRẢ KẾT QUẢ VỀ CHO CLIENT ===
                return Ok(paymentResult);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"[ERROR] Lỗi nghiêm trọng trong CreateAppointmentAndGetPaymentLink: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi hệ thống.", error = ex.Message });
            }
        }
    }
}