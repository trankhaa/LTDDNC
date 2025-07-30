// File: Controllers/HealthTipController.cs
using backend.Models.Entities;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class HealthTipController : ControllerBase
{
    private readonly HealthTipService _healthTipService;

    public HealthTipController(HealthTipService healthTipService)
    {
        _healthTipService = healthTipService;
    }

    // API cho Trang chủ: Lấy các mẹo nổi bật
    // GET: /api/HealthTip/featured
    [HttpGet("featured")]
    public async Task<ActionResult<List<HealthTip>>> GetFeatured()
    {
        var tips = await _healthTipService.GetFeaturedTipsAsync();
        return Ok(tips);
    }

    // API cho Trang 1 (Danh mục): Lấy tất cả mẹo, có thể lọc theo category
    // GET: /api/HealthTip
    // GET: /api/HealthTip?category=Dinh dưỡng
    [HttpGet]
    public async Task<ActionResult<List<HealthTip>>> GetAll([FromQuery] string? category)
    {
        var tips = await _healthTipService.GetAllAsync(category);
        return Ok(tips);
    }

    // API cho Trang 2 (Chi tiết): Lấy một mẹo theo ID
    // GET: /api/HealthTip/{id}
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<HealthTip>> GetById(string id)
    {
        var tip = await _healthTipService.GetByIdAsync(id);
        if (tip == null)
        {
            return NotFound($"Không tìm thấy mẹo sức khỏe với ID: {id}");
        }
        return Ok(tip);
    }

    // (Tùy chọn) Endpoint để tạo mới một mẹo
    // POST: /api/HealthTip
    [HttpPost]
    public async Task<IActionResult> Create(HealthTip newTip)
    {
        await _healthTipService.CreateAsync(newTip);
        // Trả về kết quả kèm theo route để lấy chi tiết và object vừa tạo
        return CreatedAtAction(nameof(GetById), new { id = newTip.Id }, newTip);
    }
}