// File: Controllers/PackagesAdminController.cs

using backend.Models.Entities;
using backend.Services.Packages;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using backend.Models.ViewModel;
using System;
using System.Linq;
using System.Collections.Generic;

namespace backend.Controllers // <<< SỬA LẠI NAMESPACE, BỎ ".Admin"
{
    // [Route("Admin/[controller]")]  // <<< COMMENT HOẶC XÓA DÒNG NÀY
    // [Authorize(Roles = "Admin")]
    public class PackagesAdminController : Controller
    {
        private readonly IPackageService _packageService;

        public PackagesAdminController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        // --- BỎ HẾT CÁC ATTRIBUTE [HttpGet], [HttpPost] TRÊN ACTION ---
        // Hệ thống sẽ tự dùng quy ước mặc định

        // GET: /PackagesAdmin/ hoặc /PackagesAdmin/Index
        public async Task<IActionResult> Index()
        {
            var packages = await _packageService.GetAllAsync();
            // Hệ thống sẽ tự tìm view tại /Views/PackagesAdmin/Index.cshtml
            return View(packages);
        }

        // GET: /PackagesAdmin/Create
        public IActionResult Create()
        {
            return View(new PackageCreateEditViewModel { IsActive = true });
        }

        // POST: /PackagesAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PackageCreateEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newPackage = new Package
                {
                    Name = model.Name,
                    Description = model.Description,
                    ImageUrl = model.ImageUrl,
                    Price = model.Price,
                    OriginalPrice = model.OriginalPrice,
                    IsActive = model.IsActive,
                    ItemsIncluded = model.ItemsIncludedString?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _packageService.CreateAsync(newPackage);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /PackagesAdmin/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var package = await _packageService.GetByIdAsync(id);
            if (package == null) return NotFound();

            var model = new PackageCreateEditViewModel
            {
                Id = package.Id,
                Name = package.Name,
                Description = package.Description,
                ImageUrl = package.ImageUrl,
                Price = package.Price,
                OriginalPrice = package.OriginalPrice,
                IsActive = package.IsActive,
                ItemsIncludedString = string.Join("\n", package.ItemsIncluded ?? new List<string>())
            };
            return View(model);
        }

        // POST: /PackagesAdmin/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, PackageCreateEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingPackage = await _packageService.GetByIdAsync(id);
                if (existingPackage == null) return NotFound();

                existingPackage.Name = model.Name;
                // ... (code gán giá trị giữ nguyên)
                await _packageService.UpdateAsync(id, existingPackage);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /PackagesAdmin/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var package = await _packageService.GetByIdAsync(id);
            if (package == null) return NotFound();
            return View(package);
        }

        // POST: /PackagesAdmin/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _packageService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}