// File: backend/Services/Hosted/ExpiredAppointmentCleanerService.cs

using MongoDB.Driver;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using backend.Models.Entities.Booking;

namespace backend.Services.Hosted
{
    public class ExpiredAppointmentCleanerService : BackgroundService
    {
        private readonly ILogger<ExpiredAppointmentCleanerService> _logger;
        private readonly IServiceProvider _services;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(10); // Chạy mỗi phút

        public ExpiredAppointmentCleanerService(ILogger<ExpiredAppointmentCleanerService> logger, IServiceProvider services)
        { 
            _logger = logger;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Đợi một chút trước khi chạy lần đầu để ứng dụng khởi động hoàn toàn
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            
            using var timer = new PeriodicTimer(_period);
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation("🚀 [Background Service] Bắt đầu quét các lịch hẹn quá hạn...");

                try
                {
                    // Tạo một scope mới để lấy dịch vụ (quan trọng!)
                    await using (var scope = _services.CreateAsyncScope())
                    {
                        var appointmentCollection = scope.ServiceProvider.GetRequiredService<IMongoCollection<ConfirmAppointment>>();

                        // Thời gian hết hạn (ví dụ: các lịch hẹn tạo trước 2 phút)
                        var expirationTime = DateTime.UtcNow.AddMinutes(-2);

                        var filter = Builders<ConfirmAppointment>.Filter.And(
                            Builders<ConfirmAppointment>.Filter.Eq(a => a.Status, PaymentStatus.PENDING_PAYMENT),
                            Builders<ConfirmAppointment>.Filter.Lt(a => a.CreatedAt, expirationTime)
                        );

                        var deleteResult = await appointmentCollection.DeleteManyAsync(filter, stoppingToken);

                        if (deleteResult.DeletedCount > 0)
                        {
                            _logger.LogInformation($"✅ [Background Service] Đã xóa {deleteResult.DeletedCount} lịch hẹn quá hạn.");
                        }
                        else
                        {
                            _logger.LogInformation("ℹ️ [Background Service] Không có lịch hẹn nào quá hạn.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ [Background Service] Gặp lỗi khi đang dọn dẹp lịch hẹn.");
                }
            }
        }
    }
}