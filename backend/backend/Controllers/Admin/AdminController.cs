using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.Services.User;
using backend.Models.Entities;
using backend.Services;
using backend.Models.ViewModel;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace backend.Controllers.Admin
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("Admin/[controller]")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;
        private readonly IBranchService _branchService;
        // private readonly IPackageService _packageService;
        private readonly IDoctorService _doctorService;

        public AdminController(IUserService userService, IDepartmentService departmentService, IBranchService branchService, IDoctorService doctorService)
        {
            _userService = userService;
            _departmentService = departmentService;
            _branchService = branchService;
            // _packageService = packageService;
            _doctorService = doctorService;
        }

        [HttpGet("Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["ErrorMessage"] = "Email và mật khẩu không được để trống.";
                return View();
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Email hoặc mật khẩu không đúng.";
                return View();
            }

            // Kiểm tra tài khoản có active không
            if (!user.IsActive)
            {
                TempData["ErrorMessage"] = "Tài khoản của bạn đã bị khóa.";
                return View();
            }

            // Kiểm tra role
            if (user.Role != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào trang quản trị.";
                return View();
            }

            // Xác thực mật khẩu
            var isValid = await _userService.ValidateUserAsync(email, password);
            if (!isValid)
            {
                TempData["ErrorMessage"] = "Email hoặc mật khẩu không đúng.";
                return View();
            }

            // Tạo claims (không có FullName nên dùng Email làm Name)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Email), // Sử dụng email thay cho name
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToLocal(returnUrl);
        }
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Admin");
        }

        [HttpGet("AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet("Statistics")]
        public async Task<IActionResult> Statistics()
        {
            var model = new backend.Models.ViewModel.AdminStatisticsViewModel
            {
                TotalPatients = await _userService.CountPatientsAsync(),
                TotalDoctors = await _doctorService.CountDoctorsAsync(),
                TotalUsers = await _userService.CountUsersAsync(),
                TotalDepartments = await _departmentService.CountDepartmentsAsync(),
                TotalBranches = await _branchService.CountBranchesAsync(),
                // TotalServices = await _packageService.CountPackagesAsync()
            };
            return View(model);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Admin");
        }
    }
}