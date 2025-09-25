using SupermarketAPI.Application.DTOs;
using SupermarketAPI.Domain.Entities;
using System.Linq;

namespace SupermarketAPI.Application.Mappers
{
    public static class CategoryMapper
    {
        public static CategoryDto ToDto(Category category)
        {
            if (category == null) return null!;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                IconUrl = category.IconUrl,
                SortOrder = category.SortOrder,
                ProductCount = category.Products?.Count ?? 0,
                SubCategories = category.SubCategories?.Select(ToDto).ToList() ?? new List<CategoryDto>()
            };
        }

        public static Category ToEntity(CategoryDto dto)
        {
            if (dto == null) return null!;

            return new Category
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                ParentCategoryId = dto.ParentCategoryId,
                IconUrl = dto.IconUrl,
                SortOrder = dto.SortOrder
            };
        }
    }
}
