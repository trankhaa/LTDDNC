using Microsoft.AspNetCore.Mvc;
using backend.Services.Chat;
using System.Security.Claims; // Cần để lấy user id nếu có xác thực

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // GET: api/chat/history/{userId}
        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetChatHistory(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            // Lấy tất cả tin nhắn mà người dùng này đã gửi hoặc nhận
            var messages = await _chatService.GetMessagesForUserAsync(userId);
            return Ok(messages);
        }
    }
}