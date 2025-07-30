// file: Models/DTOs/PackageDto.cs

using System.Collections.Generic;

namespace backend.Models.DTOs
{
    public class PackageDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public List<string> ItemsIncluded { get; set; }
    }
}