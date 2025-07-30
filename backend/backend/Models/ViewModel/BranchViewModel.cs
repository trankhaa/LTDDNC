    using Microsoft.AspNetCore.Mvc;
    using System.ComponentModel.DataAnnotations;

    namespace backend.Models.ViewModel
    {
        public class BranchViewModel
        {
            [Required(ErrorMessage = "Branch name is required")]
            [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
            public string BranchName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Address is required")]
            public string BranchAddress { get; set; } = string.Empty;

            [Phone(ErrorMessage = "Invalid phone number")]
            public string BranchHotline { get; set; } = string.Empty;

            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string BranchEmail { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public double? Latitude { get; set; }

            public double? Longitude { get; set; }

            [FromForm]
            public IFormFile? ImageFile { get; set; }
        }
    }
