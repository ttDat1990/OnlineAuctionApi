using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuction.Dtos;
using OnlineAuction.Services;

namespace OnlineAuction.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
    {
        try
        {
            var createdCategory = await _categoryService.CreateCategoryAsync(createCategoryDto);
            return Ok(createdCategory);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryService.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryDto updateCategoryDto)
    {
        try
        {
            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
            if (updatedCategory == null)
            {
                return NotFound("Category is not exists.");
            }

            return Ok(updatedCategory);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var isDeleted = await _categoryService.DeleteCategoryAsync(id);

        if (!isDeleted)
        {
            return NotFound("Category is not exists.");
        }

        return Ok(new { Result = "success" });
    }
}
