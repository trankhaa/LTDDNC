// Controllers/SpecialtyController.cs
using backend.Models.ViewModel;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public class SpecialtiesController : Controller
    {
        private readonly ISpecialtyService _specialtyService;
        private readonly IDepartmentService _departmentService;

        public SpecialtiesController(ISpecialtyService specialtyService, IDepartmentService departmentService)
        {
            _specialtyService = specialtyService;
            _departmentService = departmentService;
        }

        // GET: Specialty
        public async Task<IActionResult> Index()
        {
            var specialties = await _specialtyService.GetAllSpecialties();
            return View(specialties);
        }

        // GET: Specialty/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var specialty = await _specialtyService.GetSpecialtyById(id);
            if (specialty == null)
            {
                return NotFound();
            }

            return View(specialty);
        }

        // GET: Specialty/Create
        public async Task<IActionResult> Create()
        {
            var departments = await _departmentService.GetAllDepartments();
            ViewBag.Departments = departments;
            return View();
        }

        // POST: Specialty/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SpecialtyViewModel specialtyVM)
        {
            if (ModelState.IsValid)
            {
                await _specialtyService.CreateSpecialty(specialtyVM);
                return RedirectToAction(nameof(Index));
            }

            var departments = await _departmentService.GetAllDepartments();
            ViewBag.Departments = departments;
            return View(specialtyVM);
        }

        // GET: Specialty/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var specialty = await _specialtyService.GetSpecialtyById(id);
            if (specialty == null)
            {
                return NotFound();
            }

            var departments = await _departmentService.GetAllDepartments();
            ViewBag.Departments = departments;

            var specialtyVM = new SpecialtyViewModel
            {
                SpecialtyName = specialty.SpecialtyName,
                Description = specialty.Description,
                DepartmentId = specialty.Department.IdDepartment
            };

            ViewBag.ImageUrl = specialty.ImageUrl;
            return View(specialtyVM);
        }

        // POST: Specialty/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, SpecialtyViewModel specialtyVM)
        {
            if (ModelState.IsValid)
            {
                await _specialtyService.UpdateSpecialty(id, specialtyVM);
                return RedirectToAction(nameof(Index));
            }

            var departments = await _departmentService.GetAllDepartments();
            ViewBag.Departments = departments;
            return View(specialtyVM);
        }

        // GET: Specialty/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            var specialty = await _specialtyService.GetSpecialtyById(id);
            if (specialty == null)
            {
                return NotFound();
            }

            return View(specialty);
        }

        // POST: Specialty/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _specialtyService.DeleteSpecialty(id);
            return RedirectToAction(nameof(Index));
        }
    }
}