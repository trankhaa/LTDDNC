// File: YourProject/Controllers/Admin/DoctorsController.cs
using Microsoft.AspNetCore.Mvc;
using backend.Services; // Namespace của DoctorScheduleService (nếu nó ở đây)
using backend.Models.ViewModel.Doctor;
using System.Threading.Tasks;
using System.Collections.Generic;
using backend.Models.Entities;
using backend.Models.Entities.Doctor;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using backend.Models.DTOs.Doctor;
using System;


namespace backend.Controllers.Admin
{
    [Route("Admin/[controller]")]
    public class DoctorsController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly IDoctorDetailService _doctorDetailService;
        private readonly IDepartmentService _departmentService;
        private readonly DoctorScheduleService _scheduleService; // Sử dụng class trực tiếp
        private readonly ISpecialtyService _specialtyService;
        private readonly IBranchService _branchService;

        public DoctorsController(
            IDoctorService doctorService,
            IDoctorDetailService doctorDetailService,
            IDepartmentService departmentService,
            ISpecialtyService specialtyService,
            IBranchService branchService,
            DoctorScheduleService scheduleService) // Inject DoctorScheduleService
        {
            _doctorService = doctorService ?? throw new ArgumentNullException(nameof(doctorService));
            _doctorDetailService = doctorDetailService ?? throw new ArgumentNullException(nameof(doctorDetailService));
            _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
            _specialtyService = specialtyService ?? throw new ArgumentNullException(nameof(specialtyService));
            _branchService = branchService ?? throw new ArgumentNullException(nameof(branchService));
            _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService)); // Gán giá trị
        }

        [HttpGet("")] // Hoặc [HttpGet] nếu đây là action mặc định cho GET
        public async Task<IActionResult> Index(string? searchName, string? departmentId, string? specialtyId)
        {
            ViewBag.CurrentSearchName = searchName;
            ViewBag.CurrentDepartmentId = departmentId;
            ViewBag.CurrentSpecialtyId = specialtyId;
            await LoadFilterDropdownData();

            try
            {
                // Giả sử IDoctorService có phương thức này, nếu không, dùng GetAllAsync() hoặc tên tương tự
                var doctors = await _doctorService.GetAllAsync();

                if (doctors == null)
                {
                    return View(new List<DoctorSummaryViewModel>());
                }

                if (!string.IsNullOrEmpty(searchName))
                {
                    doctors = doctors.Where(d => d.Name != null && d.Name.ToLower().Contains(searchName.ToLower())).ToList();
                }

                var viewModels = new List<DoctorSummaryViewModel>();

                if (!string.IsNullOrEmpty(departmentId) || !string.IsNullOrEmpty(specialtyId))
                {
                    var doctorIdsToKeep = new HashSet<string>(doctors.Select(d => d.IdDoctor!));
                    var relevantDoctorDetails = new List<DoctorDetail>();

                    foreach (var doctorId in doctorIdsToKeep)
                    {
                        var detail = await _doctorDetailService.GetDoctorDetailByDoctorIdAsync(doctorId);
                        if (detail != null)
                        {
                            relevantDoctorDetails.Add(detail);
                        }
                    }

                    if (!string.IsNullOrEmpty(departmentId))
                    {
                        var doctorIdsInDepartment = relevantDoctorDetails
                            .Where(dd => dd.DepartmentId == departmentId)
                            .Select(dd => dd.DoctorId)
                            .ToHashSet();
                        doctorIdsToKeep.IntersectWith(doctorIdsInDepartment);
                    }

                    if (!string.IsNullOrEmpty(specialtyId))
                    {
                        var doctorIdsInSpecialty = relevantDoctorDetails
                            .Where(dd => dd.SpecialtyId == specialtyId)
                            .Select(dd => dd.DoctorId)
                            .ToHashSet();
                        doctorIdsToKeep.IntersectWith(doctorIdsInSpecialty);
                    }
                    doctors = doctors.Where(d => doctorIdsToKeep.Contains(d.IdDoctor)).ToList();
                }

                var allDepartmentsForLookup = (await _departmentService.GetAllDepartments())?
                   .Where(d => !string.IsNullOrEmpty(d.IdDepartment) && !string.IsNullOrEmpty(d.DepartmentName))
                   .ToDictionary(d => d.IdDepartment!, d => d.DepartmentName);

                var allSpecialtiesForLookup = (await _specialtyService.GetAllSpecialties())?
                    .Where(s => !string.IsNullOrEmpty(s.IdSpecialty) && !string.IsNullOrEmpty(s.SpecialtyName))
                    .ToDictionary(s => s.IdSpecialty!, s => s.SpecialtyName);


                foreach (var doctor in doctors)
                {
                    if (doctor == null || string.IsNullOrEmpty(doctor.IdDoctor)) continue;
                    var doctorDetail = await _doctorDetailService.GetDoctorDetailByDoctorIdAsync(doctor.IdDoctor);

                    string? departmentName = null;
                    if (doctorDetail?.DepartmentId != null && allDepartmentsForLookup != null &&
                        allDepartmentsForLookup.TryGetValue(doctorDetail.DepartmentId, out var deptName))
                    {
                        departmentName = deptName;
                    }

                    string? specialtyName = null;
                    if (doctorDetail?.SpecialtyId != null && allSpecialtiesForLookup != null &&
                        allSpecialtiesForLookup.TryGetValue(doctorDetail.SpecialtyId, out var specName))
                    {
                        specialtyName = specName;
                    }

                    viewModels.Add(new DoctorSummaryViewModel
                    {
                        DoctorId = doctor.IdDoctor,
                        Name = doctor.Name ?? "N/A",
                        Email = doctor.Email ?? "N/A",
                        AvatarUrl = doctorDetail?.Img,
                        Degree = doctorDetail?.Degree,
                        DepartmentName = departmentName,
                        SpecialtyName = specialtyName
                    });
                }
                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách bác sĩ. Chi tiết: " + ex.Message;
                await LoadFilterDropdownData();
                return View(new List<DoctorSummaryViewModel>());
            }
        }

        private async Task LoadFilterDropdownData()
        {
            ViewBag.Departments = (await _departmentService.GetAllDepartments())?
                                  .Select(d => new SelectListItem { Value = d.IdDepartment, Text = d.DepartmentName })
                                  .ToList() ?? new List<SelectListItem>();

            ViewBag.Specialties = (await _specialtyService.GetAllSpecialties())?
                                  .Select(s => new SelectListItem { Value = s.IdSpecialty, Text = s.SpecialtyName })
                                  .ToList() ?? new List<SelectListItem>();
        }

        private async Task LoadDoctorFormDataDropdowns()
        {
            // Đảm bảo GetAllBranchesAsync tồn tại và đúng trong IBranchService
            ViewBag.Branches = (await _branchService.GetAllBranches())?
                                .Select(b => new SelectListItem { Value = b.IdBranch, Text = b.BranchName })
                                .ToList() ?? new List<SelectListItem>();

            ViewBag.Departments = (await _departmentService.GetAllDepartments())?
                                  .Select(d => new SelectListItem { Value = d.IdDepartment, Text = d.DepartmentName })
                                  .ToList() ?? new List<SelectListItem>();

            ViewBag.Specialties = (await _specialtyService.GetAllSpecialties())?
                                  .Select(s => new SelectListItem { Value = s.IdSpecialty, Text = s.SpecialtyName })
                                  .ToList() ?? new List<SelectListItem>();
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await LoadDoctorFormDataDropdowns();
            var model = new CreateFullDoctorDto
            {
                DateOfBirth = DateTime.Now.AddYears(-30)
            };
            return View(model);
        }



        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFullDoctorDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var (success, errorMessage, createdDoctor) = await _doctorService.CreateDoctorWithDetailsAsync(dto);
                    if (success)
                    {
                        TempData["SuccessMessage"] = $"Bác sĩ '{createdDoctor?.Name}' đã được tạo thành công.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, errorMessage ?? "Đã có lỗi xảy ra khi tạo bác sĩ.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn trong quá trình xử lý. Vui lòng thử lại. Chi tiết: " + ex.Message);
                }
            }
            await LoadDoctorFormDataDropdowns();
            return View(dto);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID bác sĩ không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Đảm bảo IDoctorService có phương thức GetDoctorByIdAsync
                var doctor = await _doctorService.GetByIdAsync(id);
                if (doctor == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bác sĩ.";
                    return RedirectToAction(nameof(Index));
                }

                // Đảm bảo IDoctorDetailService có phương thức GetDoctorDetailByDoctorIdAsync
                var doctorDetail = await _doctorDetailService.GetDoctorDetailByDoctorIdAsync(doctor.IdDoctor!);

                // _scheduleService.GetDoctorScheduleByDoctorIdAsync trả về List<DoctorSchedule>
                List<DoctorSchedule> doctorSchedulesList = await _scheduleService.GetDoctorScheduleByDoctorIdAsync(doctor.IdDoctor!);

                // Lấy lịch khám đầu tiên từ danh sách (nếu có) để hiển thị
                DoctorSchedule? firstSchedule = doctorSchedulesList?.FirstOrDefault();

                var viewModel = new DoctorFullInfoViewModel
                {
                    DoctorId = doctor.IdDoctor,
                    Name = doctor.Name,
                    Gender = doctor.Gender.ToString(), // Chuyển enum sang string
                    DateOfBirth = doctor.DateOfBirth.ToLocalTime(),
                    Cccd = doctor.Cccd,
                    Phone = doctor.Phone,
                    Email = doctor.Email,
                    CreatedAt = doctor.CreatedAt, // Giả sử doctor.CreatedAt là DateTime và không null
                    UpdatedAt = doctor.UpdatedAt  // Giả sử doctor.UpdatedAt là DateTime và không null
                };

                if (doctorDetail != null)
                {
                    viewModel.AvatarUrl = doctorDetail.Img;
                    viewModel.CertificateImgUrl = doctorDetail.CertificateImg;
                    viewModel.DegreeImgUrl = doctorDetail.DegreeImg;
                    viewModel.Degree = doctorDetail.Degree;
                    viewModel.Description = doctorDetail.Description;
                    viewModel.BranchName = doctorDetail.BranchName;
                    viewModel.DepartmentName = doctorDetail.DepartmentName;
                    viewModel.SpecialtyName = doctorDetail.SpecialtyName;
                }

                // Gán thông tin từ firstSchedule (nếu nó không null)
                if (firstSchedule != null)
                {
                    viewModel.ConsultationFee = firstSchedule.ConsultationFee;
                    viewModel.StartTime = firstSchedule.StartTime;
                    viewModel.EndTime = firstSchedule.EndTime;
                    viewModel.ExaminationTime = firstSchedule.ExaminationTime;
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải chi tiết bác sĩ. Chi tiết: " + ex.Message;
                // Có thể ghi log lỗi ở đây: _logger.LogError(ex, "Lỗi khi xem chi tiết bác sĩ ID: {DoctorId}", id);
                return RedirectToAction(nameof(Index));
            }
        }


        // GET: Admin/Doctors/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("ID bác sĩ không hợp lệ.");
            }

            var doctor = await _doctorService.GetByIdAsync(id); // Giả sử đã có
            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bác sĩ.";
                return RedirectToAction(nameof(Index));
            }

            var doctorDetail = await _doctorDetailService.GetDoctorDetailByDoctorIdAsync(doctor.IdDoctor!);
            // DoctorScheduleService.GetDoctorScheduleByDoctorIdAsync trả về List<DoctorSchedule>
            List<DoctorSchedule> doctorSchedulesList = await _scheduleService.GetDoctorScheduleByDoctorIdAsync(doctor.IdDoctor!);
            DoctorSchedule? firstSchedule = doctorSchedulesList?.FirstOrDefault();


            if (doctorDetail == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin chi tiết của bác sĩ.";
                return RedirectToAction(nameof(Index));
            }

            var dto = new backend.Models.DTOs.Doctor.UpdateFullDoctorDto // Sử dụng UpdateFullDoctorDto
            {
                Name = doctor.Name ?? string.Empty,
                Gender = doctor.Gender, // Sửa ở đây: Giả sử DoctorGender trong DTO và Entity là cùng kiểu
                DateOfBirth = doctor.DateOfBirth,
                Cccd = doctor.Cccd ?? string.Empty,
                Phone = doctor.Phone ?? string.Empty,
                Email = doctor.Email ?? string.Empty,

                Degree = doctorDetail.Degree ?? string.Empty,
                Description = doctorDetail.Description,
                BranchId = doctorDetail.BranchId ?? string.Empty,
                DepartmentId = doctorDetail.DepartmentId ?? string.Empty,
                SpecialtyId = doctorDetail.SpecialtyId,

                ExistingAvatarUrl = doctorDetail.Img,
                ExistingCertificateImgUrl = doctorDetail.CertificateImg,
                ExistingDegreeImgUrl = doctorDetail.DegreeImg,

                // Gán từ firstSchedule
                ConsultationFee = firstSchedule?.ConsultationFee,
                StartTime = firstSchedule?.StartTime,
                EndTime = firstSchedule?.EndTime,
                ExaminationTime = firstSchedule?.ExaminationTime
            };

            await LoadDoctorFormDataDropdowns();
            return View(dto);
        }

        // POST: Admin/Doctors/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UpdateFullDoctorDto dto) // Sử dụng UpdateFullDoctorDto
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("ID bác sĩ không hợp lệ.");
            }

            if (!string.IsNullOrEmpty(dto.NewPassword) && dto.NewPassword != dto.ConfirmNewPassword)
            {
                ModelState.AddModelError("ConfirmNewPassword", "Xác nhận mật khẩu mới không khớp.");
            }
            else if (string.IsNullOrEmpty(dto.NewPassword) && ModelState.ContainsKey("ConfirmNewPassword"))
            {
                ModelState.Remove("ConfirmNewPassword");
            }


            if (ModelState.IsValid)
            {
                try
                {
                    // Giả sử IDoctorService có phương thức này
                    try
                    {
                        // Bước 2: Gọi service để thực hiện nghiệp vụ cập nhật
                        // Bạn BẮT BUỘC phải implement và gọi phương thức này trong IDoctorService/DoctorService
                        var (success, errorMessage) = await _doctorService.UpdateDoctorWithDetailsAsync(id, dto);

                        if (success) // Bước 3: Xử lý kết quả từ service
                        {
                            TempData["SuccessMessage"] = $"Thông tin bác sĩ '{dto.Name}' đã được cập nhật thành công.";
                            return RedirectToAction(nameof(Details), new { id = id }); // Chuyển hướng nếu thành công
                        }
                        else
                        {
                            // Thêm lỗi từ service vào ModelState để hiển thị cho người dùng
                            ModelState.AddModelError(string.Empty, errorMessage ?? "Có lỗi xảy ra khi cập nhật thông tin bác sĩ.");
                        }
                    }
                    catch (Exception ex) // Bắt các lỗi không lường trước từ service hoặc quá trình xử lý
                    {
                        // Log lỗi này (_logger.LogError(ex, "Lỗi khi cập nhật bác sĩ ID {DoctorId}", id);)
                        ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi hệ thống không mong muốn. Vui lòng thử lại. Chi tiết: " + ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn. " + ex.Message);
                }
            }

            await LoadDoctorFormDataDropdowns();
            // Lấy lại URL ảnh hiện tại nếu model không hợp lệ để hiển thị lại trên form
            if (!ModelState.IsValid)
            {
                var currentDoctorDetail = await _doctorDetailService.GetDoctorDetailByDoctorIdAsync(id);
                if (currentDoctorDetail != null)
                {
                    dto.ExistingAvatarUrl = string.IsNullOrEmpty(dto.ExistingAvatarUrl) && dto.ImgFile == null ? currentDoctorDetail.Img : dto.ExistingAvatarUrl;
                    dto.ExistingCertificateImgUrl = string.IsNullOrEmpty(dto.ExistingCertificateImgUrl) && dto.CertificateImgFile == null ? currentDoctorDetail.CertificateImg : dto.ExistingCertificateImgUrl;
                    dto.ExistingDegreeImgUrl = string.IsNullOrEmpty(dto.ExistingDegreeImgUrl) && dto.DegreeImgFile == null ? currentDoctorDetail.DegreeImg : dto.ExistingDegreeImgUrl;
                }
            }
            return View(dto);
        }


        [HttpGet("GetSpecialtiesByDepartment")] // Hoặc một tên route khác bạn muốn
        public async Task<JsonResult> GetSpecialtiesByDepartment(string departmentId)
        {
            if (string.IsNullOrEmpty(departmentId))
            {
                return Json(new List<SelectListItem>()); // Trả về danh sách rỗng nếu không có departmentId
            }

            try
            {
                var specialties = await _specialtyService.GetByDepartmentIdAsync(departmentId);
                var specialtyListItems = specialties.Select(s => new SelectListItem
                {
                    Value = s.IdSpecialty, // Đảm bảo IdSpecialty không null
                    Text = s.SpecialtyName
                }).ToList();

                return Json(specialtyListItems);
            }
            catch (Exception ex)
            {
                // Log lỗi ex
                // Trả về lỗi hoặc danh sách rỗng tùy theo cách bạn muốn xử lý
                Console.WriteLine($"Error fetching specialties: {ex.Message}");
                return Json(new { error = "Không thể tải danh sách chuyên khoa." }); // Hoặc danh sách rỗng
            }
        }



        // GET: Admin/Doctors/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("ID bác sĩ không hợp lệ.");
            }

            // Lấy thông tin bác sĩ để hiển thị trên trang xác nhận
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bác sĩ để xóa.";
                return RedirectToAction(nameof(Index));
            }

            // Bạn có thể tạo một ViewModel đơn giản cho trang Delete nếu muốn,
            // hoặc truyền trực tiếp entity Doctor.
            // Ví dụ, DoctorSummaryViewModel có thể đủ dùng ở đây.
            var doctorSummary = new DoctorSummaryViewModel
            {
                DoctorId = doctor.IdDoctor,
                Name = doctor.Name,
                Email = doctor.Email
                // Thêm các trường khác nếu cần hiển thị trên trang xác nhận
            };

            return View(doctorSummary); // Truyền thông tin bác sĩ cho View xác nhận
        }

        // POST: Admin/Doctors/Delete/5
        [HttpPost("Delete/{id}"), ActionName("Delete")] // ActionName("Delete") để khớp với asp-action="Delete" trong form
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID bác sĩ không hợp lệ khi xác nhận xóa.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var (success, errorMessage) = await _doctorService.DeleteDoctorAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Bác sĩ đã được xóa thành công.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = errorMessage ?? "Có lỗi xảy ra khi xóa bác sĩ.";
                    // Có thể chuyển hướng lại trang Delete với thông báo lỗi, hoặc trang Index
                    return RedirectToAction(nameof(Delete), new { id = id });
                }
            }
            catch (Exception ex)
            {
                // Log lỗi ex
                TempData["ErrorMessage"] = "Đã xảy ra lỗi hệ thống không mong muốn khi xóa. " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}