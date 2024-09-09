using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineAuction.Dtos;
using OnlineAuction.Models;

namespace OnlineAuction.Services;

public class CategoryServiceImpl : ICategoryService
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;

    public CategoryServiceImpl(DatabaseContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
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

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        var categories = await _dbContext.Categories.ToListAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(int categoryId, CreateCategoryDto updateCategoryDto)
    {
        var category = await _dbContext.Categories.FindAsync(categoryId);
        if (category == null)
        {
            return null;
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

        return _mapper.Map<CategoryDto>(category);
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
