// // File: Areas/Admin/Models/PackageCreateEditViewModel.cs

// using System.ComponentModel.DataAnnotations;
// using System.Collections.Generic;

// namespace backend.Areas.Admin.Models
// {
//     public class PackageCreateEditViewModel
//     {
//         public string? Id { get; set; }

//         [Required(ErrorMessage = "Tên gói khám không được để trống")]
//         [Display(Name = "Tên gói khám")]
//         public string Name { get; set; } = string.Empty;

//         [Required(ErrorMessage = "Mô tả không được để trống")]
//         [Display(Name = "Mô tả")]
//         public string Description { get; set; } = string.Empty;

//         [Required(ErrorMessage = "URL hình ảnh không được để trống")]
//         [Display(Name = "URL Hình ảnh")]
//         [Url(ErrorMessage = "URL hình ảnh không hợp lệ")]
//         public string ImageUrl { get; set; } = string.Empty;

//         [Required(ErrorMessage = "Giá gói khám không được để trống")]
//         [Display(Name = "Giá bán")]
//         [Range(0, double.MaxValue, ErrorMessage = "Giá phải là một số không âm")]
//         public decimal Price { get; set; }

//         [Display(Name = "Giá gốc (nếu có)")]
//         [Range(0, double.MaxValue, ErrorMessage = "Giá gốc phải là một số không âm")]
//         public decimal? OriginalPrice { get; set; }

//         [Display(Name = "Các hạng mục khám (mỗi mục một dòng)")]
//         public string ItemsIncludedString { get; set; } = string.Empty;

//         [Display(Name = "Kích hoạt")]
//         public bool IsActive { get; set; }
//     }
// }