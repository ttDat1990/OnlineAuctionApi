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

}
