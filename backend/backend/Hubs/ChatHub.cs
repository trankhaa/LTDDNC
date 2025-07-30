using backend.Models.Entities.Chat;
using backend.Services.Chat;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent; // Sử dụng ConcurrentDictionary cho an toàn luồng

namespace backend.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        // Dùng Dictionary để lưu map giữa UserId và ConnectionId của SignalR
        // Static để nó tồn tại xuyên suốt các instance của Hub
        private static readonly ConcurrentDictionary<string, string> UserConnections = new ConcurrentDictionary<string, string>();

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        // Phương thức này được gọi khi một client kết nối
        public override Task OnConnectedAsync()
        {
            // Lấy userId từ query string khi client kết nối
            var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
            if (!string.IsNullOrEmpty(userId))
            {
                // Lưu lại connectionId cho userId này
                UserConnections[userId] = Context.ConnectionId;
                Console.WriteLine($"--> Client connected: {userId} with connectionId: {Context.ConnectionId}");
            }
            return base.OnConnectedAsync();
        }

        // Phương thức này được gọi khi một client ngắt kết nối
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (userId != null)
            {
                // Xóa khỏi danh sách khi ngắt kết nối
                UserConnections.TryRemove(userId, out _);
                 Console.WriteLine($"--> Client disconnected: {userId}");
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string fromUser, string toUser, string message)
        {
            var chatMessage = new ChatMessage
            {
                From = fromUser,
                To = toUser,
                Message = message,
                Timestamp = DateTime.UtcNow // Đảm bảo gán Timestamp ở đây
            };

            await _chatService.SaveMessageAsync(chatMessage);

            // Tìm connectionId của người gửi và người nhận
            UserConnections.TryGetValue(fromUser, out var senderConnectionId);
            UserConnections.TryGetValue(toUser, out var receiverConnectionId);

            // Tạo danh sách các connectionId cần gửi tin nhắn tới
            var connectionsToNotify = new List<string>();
            if (senderConnectionId != null) connectionsToNotify.Add(senderConnectionId);
            if (receiverConnectionId != null) connectionsToNotify.Add(receiverConnectionId);

            if (connectionsToNotify.Any())
            {
                // Chỉ gửi cho người gửi và người nhận nếu họ đang online
                await Clients.Clients(connectionsToNotify)
                             .SendAsync("ReceiveMessage", fromUser, toUser, message, chatMessage.Timestamp);
            }
        }
    }
}