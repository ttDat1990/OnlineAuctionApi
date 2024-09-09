using OnlineAuction.Dtos;

namespace OnlineAuction.Services;

public interface ICategoryService
{
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
    Task<CategoryDto?> UpdateCategoryAsync(int categoryId, CreateCategoryDto updateCategoryDto);
    Task<bool> DeleteCategoryAsync(int categoryId);
}
