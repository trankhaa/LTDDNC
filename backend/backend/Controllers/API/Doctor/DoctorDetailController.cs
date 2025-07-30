using Microsoft.AspNetCore.Mvc;
using backend.Models.Entities.Doctor;
using backend.Services;
using backend.Models.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic; // Cho List
using System.Linq; // Cho .Any()
using MongoDB.Bson; // Cho ObjectId.TryParse
using System; // Cho Exception

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Route sẽ là /api/DoctorDetail
    public class DoctorDetailController : ControllerBase
    {
        private readonly DoctorDetailService _doctorDetailService;
        // private readonly ILogger<DoctorDetailController> _logger; // Cân nhắc inject logger

        public DoctorDetailController(DoctorDetailService doctorDetailService /*, ILogger<DoctorDetailController> logger */)
        {
            _doctorDetailService = doctorDetailService;
            // _logger = logger;
        }

        // GET: api/DoctorDetail/all
        [HttpGet("all")]
        [ProducesResponseType(typeof(List<DoctorDetail>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var details = await _doctorDetailService.GetAllAsync();
            return Ok(details);
        }

        // GET: api/DoctorDetail/by-doctor/{doctorId}
        [HttpGet("by-doctor/{doctorId}")]
        [ProducesResponseType(typeof(DoctorDetail), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByDoctorId(string doctorId)
        {
            if (string.IsNullOrEmpty(doctorId) || !ObjectId.TryParse(doctorId, out _))
            {
                return BadRequest(new { message = "Valid Doctor ID is required." });
            }

            var detail = await _doctorDetailService.GetDoctorDetailByDoctorIdAsync(doctorId);
            if (detail == null)
            {
                return NotFound(new { message = $"No details found for Doctor ID: {doctorId}" });
            }
            return Ok(detail);
        }

        // POST: api/DoctorDetail/create
        [HttpPost("create")]
        [ProducesResponseType(typeof(DoctorDetail), StatusCodes.Status201Created)] // Hoặc Status200OK tùy theo convention
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromForm] DoctorDetailUploadDto dto)
        {
            // Validation cơ bản cho các trường bắt buộc trong DTO
            if (string.IsNullOrEmpty(dto.DoctorId) || !ObjectId.TryParse(dto.DoctorId, out _))
                return BadRequest(new { message = "Valid DoctorId is required in the DTO." });
            if (string.IsNullOrEmpty(dto.BranchId) || !ObjectId.TryParse(dto.BranchId, out _))
                return BadRequest(new { message = "Valid BranchId is required in the DTO." });
            if (string.IsNullOrEmpty(dto.DepartmentId) || !ObjectId.TryParse(dto.DepartmentId, out _))
                return BadRequest(new { message = "Valid DepartmentId is required in the DTO." });
            // SpecialtyId có thể là tùy chọn, service sẽ xử lý nếu nó rỗng/null

            var (success, errorMessage, createdDetail) = await _doctorDetailService.CreateAsync(dto);

            if (!success)
            {
                // errorMessage có thể chứa thông tin như "Branch not found", "Specialty not found", etc.
                return BadRequest(new { message = errorMessage ?? "Failed to create doctor detail." });
            }

            // Trả về 201 Created với location header và đối tượng đã tạo
            // Hoặc đơn giản là Ok(createdDetail) nếu bạn không cần Location header
            return CreatedAtAction(nameof(GetByDoctorId), new { doctorId = createdDetail!.DoctorId }, createdDetail);
            // return Ok(new { message = "Doctor detail created successfully.", detail = createdDetail });
        }

        // PUT: api/DoctorDetail/{id}  (id ở đây là IdDoctorDetail)
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] // Hoặc Status200OK với đối tượng đã cập nhật
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string id, [FromBody] DoctorDetail detailToUpdate) // Nhận DoctorDetail đầy đủ
        {
            if (string.IsNullOrEmpty(id) || !ObjectId.TryParse(id, out _))
            {
                return BadRequest(new { message = "Valid Doctor Detail ID is required in the URL." });
            }
            if (id != detailToUpdate.IdDoctorDetail) // Đảm bảo ID trong URL khớp với ID trong body
            {
                return BadRequest(new { message = "ID in URL must match ID in request body." });
            }

            // Có thể thêm validation cho detailToUpdate ở đây

            var updated = await _doctorDetailService.UpdateAsync(id, detailToUpdate);
            if (!updated)
            {
                // Có thể là do không tìm thấy hoặc không có gì thay đổi. Service nên trả về thông tin rõ ràng hơn nếu cần.
                return NotFound(new { message = $"Doctor detail with ID {id} not found or no changes made." });
            }
            return NoContent(); // HTTP 204 No Content là chuẩn cho PUT thành công không trả về body
            // Hoặc: return Ok(new { message = "Doctor detail updated successfully." });
        }

        // DELETE: api/DoctorDetail/{id} (id ở đây là IdDoctorDetail)
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id) || !ObjectId.TryParse(id, out _))
            {
                return BadRequest(new { message = "Valid Doctor Detail ID is required." });
            }

            var deleted = await _doctorDetailService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Doctor detail with ID {id} not found." });
            }
            return NoContent(); // HTTP 204 No Content là chuẩn cho DELETE thành công
            // Hoặc: return Ok(new { message = "Doctor detail deleted successfully." });
        }


        // ***** ACTION TÌM KIẾM ĐÃ THÊM VÀO ĐÂY *****
        // GET: api/DoctorDetail/search-by-criteria?branchId=xxx&departmentId=yyy&specialtyId=zzz
        [HttpGet("search-by-criteria")]
        [ProducesResponseType(typeof(List<DoctorSearchResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<DoctorSearchResultDto>>> SearchDoctorsByCriteria(
            [FromQuery] string branchId,
            [FromQuery] string departmentId,
            [FromQuery] string? specialtyId = null)
        {
            if (string.IsNullOrEmpty(branchId))
                return BadRequest(new { message = "Branch ID is required." });
            if (string.IsNullOrEmpty(departmentId))
                return BadRequest(new { message = "Department ID is required." });

            if (!ObjectId.TryParse(branchId, out _))
                return BadRequest(new { message = $"Invalid Branch ID format: {branchId}" });
            if (!ObjectId.TryParse(departmentId, out _))
                return BadRequest(new { message = $"Invalid Department ID format: {departmentId}" });
            if (!string.IsNullOrEmpty(specialtyId) && !ObjectId.TryParse(specialtyId, out _))
                return BadRequest(new { message = $"Invalid Specialty ID format: {specialtyId}" });

            try
            {
                var doctors = await _doctorDetailService.FindDoctorsByCriteriaAsync(branchId, departmentId, specialtyId);

                if (doctors == null || !doctors.Any())
                {
                    return NotFound(new { message = "No doctors found matching the specified criteria." });
                }
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "An error occurred while searching doctors by criteria.");
                Console.WriteLine($"Error in SearchDoctorsByCriteria: {ex.Message} \n {ex.StackTrace}"); // Tạm thời cho debug
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred. Please try again later." });
            }
        }


        [HttpPost("search-by-criteria")]
        public async Task<IActionResult> SearchDoctorsByCriteria([FromBody] SearchDoctorCriteriaDto criteria)
        {
            var result = await _doctorDetailService.SearchDoctorsByCriteria(criteria);
            return Ok(result);
        }


        // ======================================================================================
        // === DÁN ĐOẠN CODE NÀY VÀO TRONG CLASS DoctorDetailController ===
        // ======================================================================================

        // GET: api/doctordetail/full-info/{doctorId}
        [HttpGet("full-info/{doctorId}")]
        [ProducesResponseType(typeof(DoctorFullInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFullInfo(string doctorId)
        {
            // Bước 1: Kiểm tra tính hợp lệ của doctorId đầu vào.
            if (string.IsNullOrEmpty(doctorId) || !MongoDB.Bson.ObjectId.TryParse(doctorId, out _))
            {
                return BadRequest(new { message = "Valid Doctor ID is required and must be a 24-digit hex string." });
            }

            // Bước 2: Gọi đến service để lấy dữ liệu.
            var fullInfo = await _doctorDetailService.GetDoctorFullInfoAsync(doctorId);

            // Bước 3: Xử lý kết quả từ service.
            if (fullInfo == null)
            {
                // Nếu service trả về null, có nghĩa là không tìm thấy bác sĩ hoặc thông tin chi tiết.
                // Trả về lỗi 404 Not Found cho client.
                return NotFound(new { message = $"Doctor with ID {doctorId} not found or has incomplete data." });
            }

            // Nếu tìm thấy, trả về 200 OK cùng với dữ liệu đầy đủ.
            return Ok(fullInfo);
        }

    }
}