using backend.Models.ViewModel;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("Admin/[controller]")]
    public class DepartmentsController : Controller
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // GET: Admin/Departments
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var departments = await _departmentService.GetAllDepartments();
            return View(departments);
        }

        // GET: Admin/Departments/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var department = await _departmentService.GetDepartmentById(id);
            if (department == null)
            {
                return NotFound();
            }

            var specialties = await _departmentService.GetSpecialtiesByDepartment(id);
            ViewBag.Specialties = specialties;

            return View(department);
        }

        // GET: Admin/Departments/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Departments/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel departmentVM)
        {
            if (ModelState.IsValid)
            {
                await _departmentService.CreateDepartment(departmentVM);
                return RedirectToAction(nameof(Index));
            }
            return View(departmentVM);
        }

        // GET: Admin/Departments/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var department = await _departmentService.GetDepartmentById(id);
            if (department == null)
            {
                return NotFound();
            }

            var departmentVM = new DepartmentViewModel
            {
                DepartmentName = department.DepartmentName,
                Description = department.Description
            };

            ViewBag.ImageUrl = department.ImageUrl;
            return View(departmentVM);
        }

        // POST: Admin/Departments/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, DepartmentViewModel departmentVM)
        {
            if (ModelState.IsValid)
            {
                await _departmentService.UpdateDepartment(id, departmentVM);
                return RedirectToAction(nameof(Index));
            }
            return View(departmentVM);
        }

        // GET: Admin/Departments/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var department = await _departmentService.GetDepartmentById(id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Admin/Departments/Delete/5
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _departmentService.DeleteDepartment(id);
            return RedirectToAction(nameof(Index));
        }
    }
}