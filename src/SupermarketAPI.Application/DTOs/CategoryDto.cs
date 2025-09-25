using System.Collections.Generic;

namespace SupermarketAPI.Application.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public string IconUrl { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public int ProductCount { get; set; }
        public List<CategoryDto> SubCategories { get; set; } = new List<CategoryDto>();
    }
}
