using Microsoft.EntityFrameworkCore;
using OnlineAuction.Dtos;
using OnlineAuction.Models;

namespace OnlineAuction.Services;

public class CategoryServiceImpl : ICategoryService
{
    private readonly DatabaseContext _dbContext;

    public CategoryServiceImpl(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
    {
        var existingCategory = await _dbContext.Categories
                .SingleOrDefaultAsync(c => c.CategoryName == createCategoryDto.CategoryName);

        if (existingCategory != null)
        {
            throw new InvalidOperationException("Category Name already exists.");
        }

        var category = new Category
        {
            CategoryName = createCategoryDto.CategoryName
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        var categories = await _dbContext.Categories
        .Select(category => new CategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            ItemCount = _dbContext.Items.Count(item => item.CategoryId == category.CategoryId)  // Count items for each category
        })
        .ToListAsync();

        return categories;
    }

    public async Task<bool> UpdateCategoryAsync(int categoryId, CreateCategoryDto updateCategoryDto)
    {
        var category = await _dbContext.Categories.FindAsync(categoryId);
        if (category == null)
        {
            return false;
        }

        var existingCategory = await _dbContext.Categories
            .SingleOrDefaultAsync(c => c.CategoryName == updateCategoryDto.CategoryName && c.CategoryId != categoryId);

        if (existingCategory != null)
        {
            throw new InvalidOperationException("Category Name already exists.");
        }

        category.CategoryName = updateCategoryDto.CategoryName;

        _dbContext.Categories.Update(category);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int categoryId)
    {
        var category = await _dbContext.Categories.FindAsync(categoryId);
        if (category == null)
        {
            return false;
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MergeCategoriesAsync(int targetCategoryId, List<int> sourceCategoryIds)
    {
        // Lấy danh mục đích
        var targetCategory = await _dbContext.Categories.FindAsync(targetCategoryId);
        if (targetCategory == null)
        {
            throw new InvalidOperationException("Target category does not exist.");
        }

        // Lấy các danh mục nguồn để gộp
        var sourceCategories = await _dbContext.Categories
            .Where(c => sourceCategoryIds.Contains(c.CategoryId))
            .ToListAsync();

        if (!sourceCategories.Any())
        {
            throw new InvalidOperationException("No valid source categories found.");
        }

        // Di chuyển tất cả items từ các danh mục nguồn sang danh mục đích
        foreach (var sourceCategory in sourceCategories)
        {
            var items = await _dbContext.Items
                .Where(item => item.CategoryId == sourceCategory.CategoryId)
                .ToListAsync();

            foreach (var item in items)
            {
                item.CategoryId = targetCategoryId; // Cập nhật CategoryId của items sang danh mục đích
            }

            // Sau khi chuyển items, xóa danh mục nguồn
            _dbContext.Categories.Remove(sourceCategory);
        }

        // Lưu các thay đổi
        await _dbContext.SaveChangesAsync();

        return true;
    }


}
