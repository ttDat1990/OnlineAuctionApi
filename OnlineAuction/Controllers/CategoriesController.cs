﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuction.Dtos;
using OnlineAuction.Services;

namespace OnlineAuction.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;

    public CategoriesController(ICategoryService categoryService, IItemService itemService)
    {
        _categoryService = categoryService;
        _itemService = itemService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
    {
        try
        {
            await _categoryService.CreateCategoryAsync(createCategoryDto);
            return Ok(new { Result = "success" });
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
            await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);

            return Ok(new { Result = "success" });
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

    [HttpGet("{id}/items")]
    public IActionResult GetItemsByCategory(int id)
    {
        var items = _itemService.GetItemsByCategory(id);

        if (items == null || items.Count == 0)
        {
            return NotFound(new { Message = "There are no items in this category." });
        }

        return Ok(items);
    }

    [HttpPost("merge")]
    public async Task<IActionResult> MergeCategories([FromBody] MergeCategoriesDto mergeCategoriesDto)
    {
        try
        {
            bool result = await _categoryService.MergeCategoriesAsync(mergeCategoriesDto.TargetCategoryId, mergeCategoriesDto.SourceCategoryIds);
            if (result)
            {
                return Ok(new { message = "Categories merged successfully." });
            }
            return BadRequest(new { message = "Failed to merge categories." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
