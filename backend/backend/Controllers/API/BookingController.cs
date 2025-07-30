using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        [HttpGet] // <<< THÊM DÒNG NÀY VÀO
        public IActionResult Index()
        {
            // Logic để lấy danh sách các booking
            return Ok("Danh sách các booking");
        }
    }
}
