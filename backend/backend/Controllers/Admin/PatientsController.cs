using backend.Services.Patient;
using backend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [Route("Admin/[controller]")]
    public class PatientsController : Controller
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        // GET: Admin/Patients
        [HttpGet]
        public async Task<IActionResult> Index(string searchString)
        {
            var patients = await _patientService.GetAllPatientsAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                patients = patients.FindAll(p =>
                    p.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    p.PatientCode.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    p.Phone.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            return View(patients);
        }

        // GET: Admin/Patients/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var patient = await _patientService.GetPatientByIdAsync(id);
                return View(patient);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: Admin/Patients/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Patients/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _patientService.CreatePatientAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(model);
        }

        // GET: Admin/Patients/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var patient = await _patientService.GetPatientByIdAsync(id);
                var model = new PatientUpdateViewModel
                {
                    FullName = patient.FullName,
                    Gender = patient.Gender,
                    DateOfBirth = patient.DateOfBirth,
                    Phone = patient.Phone,
                    Address = patient.Address,
                    PatientCode = patient.PatientCode,
                    BloodType = patient.BloodType,
                    Height = patient.Height,
                    Weight = patient.Weight,
                    InsuranceNumber = patient.InsuranceNumber,
                    MedicalHistory = patient.MedicalHistory,
                    Allergies = patient.Allergies
                };
                return View(model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: Admin/Patients/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, PatientUpdateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _patientService.UpdatePatientAsync(id, model);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(model);
        }

        // GET: Admin/Patients/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var patient = await _patientService.GetPatientByIdAsync(id);
                return View(patient);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: Admin/Patients/Delete/5
        [HttpPost("Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _patientService.DeletePatientAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}