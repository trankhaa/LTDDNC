// Controllers/BranchController.cs
using backend.Models.ViewModel;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public class BranchesController : Controller
    {
        private readonly IBranchService _branchService;

        public BranchesController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        // GET: Branch
        public async Task<IActionResult> Index()
        {
            var branches = await _branchService.GetAllBranches();
            return View(branches);
        }

        // GET: Branch/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var branch = await _branchService.GetBranchById(id);
            if (branch == null)
            {
                return NotFound();
            }

            return View(branch);
        }

        // GET: Branch/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Branch/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BranchViewModel branchVM)
        {
            if (ModelState.IsValid)
            {
                await _branchService.CreateBranch(branchVM);
                return RedirectToAction(nameof(Index));
            }
            return View(branchVM);
        }

        // GET: Branch/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var branch = await _branchService.GetBranchById(id);
            if (branch == null)
            {
                return NotFound();
            }

            var branchVM = new BranchViewModel
            {
                BranchName = branch.BranchName,
                BranchAddress = branch.BranchAddress,
                BranchHotline = branch.BranchHotline,
                BranchEmail = branch.BranchEmail,
                Description = branch.Description,
                Latitude = branch.Coordinates?.Latitude,
                Longitude = branch.Coordinates?.Longitude
            };

            ViewBag.ImageUrl = branch.ImageUrl;
            return View(branchVM);
        }

        // POST: Branch/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, BranchViewModel branchVM)
        {
            if (ModelState.IsValid)
            {
                await _branchService.UpdateBranch(id, branchVM);
                return RedirectToAction(nameof(Index));
            }
            return View(branchVM);
        }

        // GET: Branch/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            var branch = await _branchService.GetBranchById(id);
            if (branch == null)
            {
                return NotFound();
            }

            return View(branch);
        }

        // POST: Branch/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _branchService.DeleteBranch(id);
            return RedirectToAction(nameof(Index));
        }
    }
}