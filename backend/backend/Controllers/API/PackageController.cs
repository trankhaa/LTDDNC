// File: Controllers/API/PackageController.cs

using backend.Models.DTOs;
using backend.Services.Packages;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")] // URL sẽ là /api/package
    public class PackageController : ControllerBase
    {
        private readonly IPackageService _packageService;

        // Inject service đã tạo trước đó
        public PackageController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        /// <summary>
        /// API để lấy danh sách tất cả các gói khám đang hoạt động.
        /// Dành cho ứng dụng di động Flutter sử dụng.
        /// </summary>
        /// <returns>Một danh sách các gói khám.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PackageDto>>> GetActivePackages()
        {
            // Tái sử dụng logic từ service
            var packages = await _packageService.GetActiveAsync();

            // Nếu không có gói nào, trả về một danh sách rỗng (vẫn là 200 OK)
            if (packages == null || !packages.Any())
            {
                return Ok(new List<PackageDto>());
            }

            // Chuyển đổi từ Entity (dữ liệu thô) sang DTO (dữ liệu sạch cho client)
            var packageDtos = packages.Select(p => new PackageDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                Price = p.Price,
                OriginalPrice = p.OriginalPrice,
                ItemsIncluded = p.ItemsIncluded
            }).ToList();

            // Trả về kết quả thành công (HTTP 200 OK) cùng với dữ liệu
            return Ok(packageDtos);
        }
    }
}