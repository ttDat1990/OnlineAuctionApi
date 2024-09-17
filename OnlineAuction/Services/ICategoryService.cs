using OnlineAuction.Dtos;

namespace OnlineAuction.Services;

public interface ICategoryService
{
    Task<bool> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
    Task<bool> UpdateCategoryAsync(int categoryId, CreateCategoryDto updateCategoryDto);
    Task<bool> DeleteCategoryAsync(int categoryId);
}
