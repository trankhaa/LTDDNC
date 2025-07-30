using Microsoft.AspNetCore.Mvc;
using backend.Models.Entities.Doctor;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorScheduleController : ControllerBase
    {
        private readonly DoctorScheduleService _scheduleService;

        public DoctorScheduleController(DoctorScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll() =>
            Ok(await _scheduleService.GetAllAsync());

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetByDoctorId(string doctorId)
        {
            var schedules = await _scheduleService.GetDoctorScheduleByDoctorIdAsync(doctorId);
            return Ok(schedules);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule == null)
                return NotFound();
            return Ok(schedule);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] DoctorSchedule schedule)
        {
            if (string.IsNullOrEmpty(schedule.DoctorId))
                return BadRequest("DoctorId is required.");

            await _scheduleService.AddAsync(schedule);
            return Ok(new { message = "Schedule created successfully", schedule });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] DoctorSchedule schedule)
        {
            var updated = await _scheduleService.UpdateAsync(id, schedule);
            if (!updated) return NotFound("Schedule not found.");
            return Ok(new { message = "Updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _scheduleService.DeleteAsync(id);
            if (!deleted) return NotFound("Schedule not found.");
            return Ok(new { message = "Deleted successfully" });
        }
    }
}