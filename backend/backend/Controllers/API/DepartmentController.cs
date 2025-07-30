
using Microsoft.AspNetCore.Mvc;
using backend.Models.ViewModel;
using backend.Services;

namespace backend.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // GET: api/department
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _departmentService.GetAllDepartments();
            return Ok(departments);
        }

        // GET: api/department/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var department = await _departmentService.GetDepartmentById(id);
            if (department == null)
                return NotFound();

            return Ok(department);
        }

        // POST: api/department
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] DepartmentViewModel departmentVM)
        {
            try
            {
                var createdDepartment = await _departmentService.CreateDepartment(departmentVM);
                return CreatedAtAction(nameof(GetById), new { id = createdDepartment.IdDepartment }, createdDepartment);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating department: {ex.Message}");
            }
        }

        // PUT: api/department/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromForm] DepartmentViewModel departmentVM)
        {
            try
            {
                await _departmentService.UpdateDepartment(id, departmentVM);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating department: {ex.Message}");
            }
        }

        // DELETE: api/department/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _departmentService.DeleteDepartment(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting department: {ex.Message}");
            }
        }

        // GET: api/department/{id}/specialties
        [HttpGet("{id}/specialties")]
        public async Task<IActionResult> GetSpecialtiesByDepartment(string id)
        {
            var specialties = await _departmentService.GetSpecialtiesByDepartment(id);
            return Ok(specialties);
        }
    }
}
