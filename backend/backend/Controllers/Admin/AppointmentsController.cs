using backend.Services;
using backend.ViewModels.Booking;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using backend.Models.Entities.Booking;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace backend.Controllers
{
    [Route("Admin/[controller]")]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string searchTerm, string doctorId, DateTime? filterDate)
        {
            IEnumerable<ConfirmAppointment> appointments;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                appointments = await _appointmentService.SearchAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm;
            }
            else if (!string.IsNullOrEmpty(doctorId) && filterDate.HasValue)
            {
                var byDoctor = await _appointmentService.FilterByDoctorAsync(doctorId);
                appointments = byDoctor.Where(a => a.Date.Date == filterDate.Value.Date).ToList();
                ViewBag.FilterDoctorId = doctorId;
                ViewBag.FilterDate = filterDate;
            }
            else if (!string.IsNullOrEmpty(doctorId))
            {
                appointments = await _appointmentService.FilterByDoctorAsync(doctorId);
                ViewBag.FilterDoctorId = doctorId;
            }
            else if (filterDate.HasValue)
            {
                appointments = await _appointmentService.FilterByDateAsync(filterDate.Value);
                ViewBag.FilterDate = filterDate;
            }
            else
            {
                appointments = await _appointmentService.GetAllAsync();
            }

            // In a real application, you would get this from a doctors service
            ViewBag.Doctors = new List<SelectListItem>
            {
                new SelectListItem { Value = "dr1", Text = "Dr. Smith" },
                new SelectListItem { Value = "dr2", Text = "Dr. Johnson" },
                new SelectListItem { Value = "dr3", Text = "Dr. Williams" }
            };

            return View(appointments);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            // Populate dropdowns or other view data if needed
            ViewBag.Doctors = new List<SelectListItem>
            {
                new SelectListItem { Value = "dr1", Text = "Dr. Smith" },
                new SelectListItem { Value = "dr2", Text = "Dr. Johnson" },
                new SelectListItem { Value = "dr3", Text = "Dr. Williams" }
            };

            return View();
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConfirmAppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var appointment = new ConfirmAppointment
                {
                    NameDr = model.NameDr,
                    DoctorId = model.DoctorId,
                    Slot = model.Slot,
                    PatientId = model.PatientId,
                    PatientEmail = model.PatientEmail,
                    Date = model.Date,
                    Symptoms = model.Symptoms
                };

                await _appointmentService.CreateAsync(appointment);
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            ViewBag.Doctors = new List<SelectListItem>
            {
                new SelectListItem { Value = "dr1", Text = "Dr. Smith" },
                new SelectListItem { Value = "dr2", Text = "Dr. Johnson" },
                new SelectListItem { Value = "dr3", Text = "Dr. Williams" }
            };

            return View(model);
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var model = new ConfirmAppointmentViewModel
            {
                IdConfirmAppointment = appointment.IdConfirmAppointment,
                NameDr = appointment.NameDr,
                DoctorId = appointment.DoctorId,
                Slot = appointment.Slot,
                PatientId = appointment.PatientId,
                PatientEmail = appointment.PatientEmail,
                Date = appointment.Date,
                Symptoms = appointment.Symptoms
            };

            ViewBag.Doctors = new List<SelectListItem>
            {
                new SelectListItem { Value = "dr1", Text = "Dr. Smith" },
                new SelectListItem { Value = "dr2", Text = "Dr. Johnson" },
                new SelectListItem { Value = "dr3", Text = "Dr. Williams" }
            };

            return View(model);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ConfirmAppointmentViewModel model)
        {
            if (id != model.IdConfirmAppointment)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingAppointment = await _appointmentService.GetByIdAsync(id);
                if (existingAppointment == null)
                {
                    return NotFound();
                }

                existingAppointment.NameDr = model.NameDr;
                existingAppointment.DoctorId = model.DoctorId;
                existingAppointment.Slot = model.Slot;
                existingAppointment.PatientId = model.PatientId;
                existingAppointment.PatientEmail = model.PatientEmail;
                existingAppointment.Date = model.Date;
                existingAppointment.Symptoms = model.Symptoms;

                await _appointmentService.UpdateAsync(id, existingAppointment);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Doctors = new List<SelectListItem>
            {
                new SelectListItem { Value = "dr1", Text = "Dr. Smith" },
                new SelectListItem { Value = "dr2", Text = "Dr. Johnson" },
                new SelectListItem { Value = "dr3", Text = "Dr. Williams" }
            };

            return View(model);
        }

        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _appointmentService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}