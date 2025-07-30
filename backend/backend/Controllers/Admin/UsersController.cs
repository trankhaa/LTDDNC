    using backend.Models.ViewModel;
    using backend.Services.User;
    using Microsoft.AspNetCore.Mvc;
    // using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;

    namespace backend.Controllers.Admin
    {
        // [Authorize(Roles = "Admin")]
        [Route("Admin/[controller]")]
        public class UsersController : Controller
        {
            private readonly IUserService _userService;

            public UsersController(IUserService userService)
            {
                _userService = userService;
            }

            [HttpGet]
            public async Task<IActionResult> Index(string? role = null)
            {
                ViewBag.CurrentRole = role;
                ViewBag.Roles = new[] { "Admin", "Doctor", "Patient" };

                try
                {
                    var users = string.IsNullOrEmpty(role)
                        ? await _userService.GetAllUsersAsync()
                        : await _userService.GetUsersByRoleAsync(role);

                    return View(users ?? new List<backend.Models.Entities.User>());
                }
                catch (Exception ex)
                {
                    // Log lỗi
                    Console.WriteLine($"Error retrieving users: {ex.Message}");
                    TempData["ErrorMessage"] = "An error occurred while loading users.";
                    return View(new List<backend.Models.Entities.User>());
                }
            }

            [HttpGet("Details/{id}")]
            public async Task<IActionResult> Details(string id)
            {
                if (string.IsNullOrEmpty(id)) return NotFound();

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null) return NotFound();

                return View(user);
            }

            [HttpGet("Create")]
            public IActionResult Create()
            {
                return View(new UserCreateEditViewModel());
            }

            [HttpPost("Create")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(UserCreateEditViewModel model)
            {
                if (!await _userService.IsEmailUniqueAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "This email is already in use. Please choose another one.");
                }

                if (ModelState.IsValid)
                {
                    await _userService.CreateUserAsync(model);
                    TempData["SuccessMessage"] = "User created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                return View(model);
            }

            [HttpGet("Edit/{id}")]
            public async Task<IActionResult> Edit(string id)
            {
                if (string.IsNullOrEmpty(id)) return NotFound();

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null) return NotFound();

                var viewModel = new UserCreateEditViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role
                };

                return View(viewModel);
            }

            [HttpPost("Edit/{id}")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(string id, UserCreateEditViewModel model)
            {
                if (id != model.Id) return BadRequest();

                if (string.IsNullOrEmpty(model.Password))
                {
                    ModelState.Remove(nameof(model.Password));
                    ModelState.Remove(nameof(model.ConfirmPassword));
                }

                if (!await _userService.IsEmailUniqueAsync(model.Email, model.Id))
                {
                    ModelState.AddModelError("Email", "This email is already in use by another user.");
                }

                if (ModelState.IsValid)
                {
                    var result = await _userService.UpdateUserAsync(model);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "User updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    ModelState.AddModelError(string.Empty, "An error occurred while updating. Please try again.");
                }

                return View(model);
            }

            [HttpGet("Delete/{id}")]
            public async Task<IActionResult> Delete(string id)
            {
                if (string.IsNullOrEmpty(id)) return NotFound();
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null) return NotFound();

                return View(user);
            }

            [HttpPost("Delete/{id}"), ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(string id)
            {
                await _userService.DeleteUserAsync(id);
                TempData["SuccessMessage"] = "User deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
        }
    }