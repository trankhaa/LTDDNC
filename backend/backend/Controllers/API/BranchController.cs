// File: BranchController.cs

using Microsoft.AspNetCore.Mvc;
using backend.Services; // Đảm bảo bạn đã using namespace của service

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchController : ControllerBase
    {
        // Thay đổi ở đây: kiểu dữ liệu là IBranchService
        private readonly IBranchService _branchService;

        // Thay đổi ở đây: tham số là IBranchService
        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var branch = await _branchService.GetBranchById(id); // ✅ sửa tên
            if (branch == null)
                return NotFound();

            return Ok(branch);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllBranches()
        {
            var branches = await _branchService.GetAllBranches(); // ✅ sửa tên
            return Ok(branches);
        }

    }
}