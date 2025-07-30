    using backend.Services;
    using backend.Models.ViewModel;
    using Microsoft.AspNetCore.Mvc;
    using backend.Models.DTOs;
    using backend.Models.Entities;

    namespace backend.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class SpecialtyController : ControllerBase
        {
            private readonly ISpecialtyService _specialtyService;

            public SpecialtyController(ISpecialtyService specialtyService)
            {
                _specialtyService = specialtyService;
            }

            [HttpPost("create")]
            public async Task<IActionResult> CreateSpecialty([FromForm] SpecialtyDTOs dto)
            {
                if (dto == null || string.IsNullOrEmpty(dto.DepartmentId))
                    return BadRequest("Specialty data is invalid");

                var model = new SpecialtyViewModel
                {
                    SpecialtyName = dto.SpecialtyName,
                    Description = dto.Description,
                    DepartmentId = dto.DepartmentId,
                    ImageFile = dto.ImageFile
                };

                try
                {
                    var result = await _specialtyService.CreateSpecialty(model);
                    return Ok(new { message = "Specialty created successfully", specialty = result });
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            [HttpGet("all")]
            public async Task<IActionResult> GetAllSpecialties()
            {
                var specialties = await _specialtyService.GetAllSpecialties();
                return Ok(specialties);
            }

            [HttpGet("by-department/{idDepartment}")]
            public async Task<IActionResult> GetSpecialtiesByDepartment(string idDepartment)
            {
                if (string.IsNullOrEmpty(idDepartment))
                    return BadRequest("IdDepartment is required");

                var specialties = await _specialtyService.GetByDepartmentIdAsync(idDepartment);

                if (specialties == null || !specialties.Any())
                    return NotFound("No specialties found for this department");

                return Ok(specialties);
            }
        }
    }
