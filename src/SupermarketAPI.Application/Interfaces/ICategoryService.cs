using SupermarketAPI.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupermarketAPI.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(int parentCategoryId);
        Task<CategoryDto> CreateCategoryAsync(CategoryDto category);
        Task<CategoryDto> UpdateCategoryAsync(CategoryDto category);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
