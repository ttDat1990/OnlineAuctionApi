using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuction.Dtos;
using OnlineAuction.Services;
using System.Security.Claims;

namespace OnlineAuction.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemsController : Controller
{
    private readonly IItemService _itemService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileService _fileService;
    private readonly AuctionStatusUpdateService _statusUpdateService;

    public ItemsController(IItemService itemService, IWebHostEnvironment webHostEnvironment, IFileService fileService, AuctionStatusUpdateService statusUpdateService)
    {
        _itemService = itemService;
        _webHostEnvironment = webHostEnvironment;
        _fileService = fileService;
        _statusUpdateService = statusUpdateService;
    }

    [HttpGet("{id}")]
    public IActionResult GetItemById(int id)
    {
        var item = _itemService.GetItemById(id);

        if (item == null)
        {
            return NotFound(new { Message = "Item does not exist." });
        }

        return Ok(item);
    }

    // Tạo sản phẩm mới, đồng thời lưu trữ hình ảnh và tài liệu
    [HttpPost("create")]
    [Authorize(Roles = "NormalUser")]
    public async Task<IActionResult> Create([FromForm] CreateItemWithFilesDto createItemWithFilesDto)
    {
        try
        {
            // Lấy ID người bán từ token
            var sellerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Khai báo dung lượng tối đa và các đuôi file hợp lệ
            long maxFileSize = 2 * 1024 * 1024; // 2MB
            List<string> allowedImageExtensions = new List<string> { ".png", ".gif", ".jpg" };
            List<string> allowedDocumentExtensions = new List<string> { ".doc", ".docx", ".xls", ".xlsx", ".txt" };

            // Lưu hình ảnh
            var imagePaths = createItemWithFilesDto.Images != null
                ? await _fileService.SaveFilesAsync(createItemWithFilesDto.Images, "images", maxFileSize, allowedImageExtensions)
                : new List<string>();

            // Lưu tài liệu
            var documentPaths = createItemWithFilesDto.Documents != null
                ? await _fileService.SaveFilesAsync(createItemWithFilesDto.Documents, "documents", maxFileSize, allowedDocumentExtensions)
                : new List<string>();

            // Gọi service để lưu sản phẩm và liên kết hình ảnh, tài liệu
            var isCreated = _itemService.CreateItem(createItemWithFilesDto, sellerId, imagePaths, documentPaths);

            if (isCreated)
            {
                // Gọi UpdateBidStatuses sau khi thêm item thành công
                await _statusUpdateService.TriggerUpdateBidStatuses();
                return Ok(new { Message = "Create successfully." });
            }
            else
            {
                return BadRequest(new { Message = "No changes were made." });
            }

        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "An error occurred: " + ex.Message });
        }
    }

    [HttpPut("update/{id}")]
    [Authorize(Roles = "NormalUser")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateItemWithFilesDto updateItemWithFilesDto)
    {
        try
        {
            // Lấy ID người bán từ token
            var sellerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Khai báo dung lượng tối đa và các đuôi file hợp lệ
            long maxFileSize = 2 * 1024 * 1024; // 2MB
            List<string> allowedImageExtensions = new List<string> { ".png", ".gif", ".jpeg" };
            List<string> allowedDocumentExtensions = new List<string> { ".doc", ".docx", ".xls", ".xlsx", ".txt" };

            // Xử lý hình ảnh mới nếu có
            var imagePaths = updateItemWithFilesDto.Images != null
                ? await _fileService.SaveFilesAsync(updateItemWithFilesDto.Images, "images", maxFileSize, allowedImageExtensions)
                : new List<string>();

            // Xử lý tài liệu mới nếu có
            var documentPaths = updateItemWithFilesDto.Documents != null
                ? await _fileService.SaveFilesAsync(updateItemWithFilesDto.Documents, "documents", maxFileSize, allowedDocumentExtensions)
                : new List<string>();

            // Gọi service để cập nhật sản phẩm
            var isUpdated = _itemService.UpdateItem(id, updateItemWithFilesDto, sellerId, imagePaths, documentPaths);

            if (isUpdated)
            {
                return Ok(new { Message = "Update successfully." });
            }
            else
            {
                return BadRequest(new { Message = "No changes were made." });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "An error occurred: " + ex.Message });
        }
    }

    [HttpDelete("delete/{id}")]
    [Authorize(Roles = "NormalUser, Admin")]
    public IActionResult Delete(int id)
    {
        try
        {
            // Tìm sản phẩm theo ID
            var item = _itemService.GetItemById(id);
            if (item == null)
            {
                return NotFound(new { Message = "Item does not exist." });
            }

            var isDeleted = _itemService.DeleteItem(id);
            if (isDeleted)
            {
                return Ok(new { Message = "Item deleted successfully." });
            }
            else
            {
                return BadRequest(new { Message = "Item deletion failed." });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred: " + ex.Message });
        }
    }

    [HttpGet("search")]
    public IActionResult SearchItems(
    [FromQuery] string? query,
    [FromQuery] int? categoryId,
    [FromQuery] int? sellerId,
    [FromQuery] string? bidStatus,
    [FromQuery] DateTime? bidStartDate,
    [FromQuery] DateTime? bidEndDate)
    {
        var items = _itemService.SearchItems(query, categoryId, sellerId, bidStatus, bidStartDate, bidEndDate);

        if (items == null || items.Count == 0)
        {
            return NotFound(new { Message = "No items were found matching the search criteria." });
        }

        return Ok(items);
    }

    [HttpGet]
    public IActionResult GetAllItems()
    {
        var items = _itemService.GetAllItems();

        if (items == null || items.Count == 0)
        {
            return NotFound(new { Message = "No items found." });
        }

        return Ok(items);
    }



}
