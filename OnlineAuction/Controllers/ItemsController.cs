using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuction.Dtos;
using OnlineAuction.Helpers;
using OnlineAuction.Services;
using System.Security.Claims;

namespace OnlineAuction.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemsController : Controller
{
    private readonly IItemService _itemService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ItemsController(IItemService itemService, IWebHostEnvironment webHostEnvironment)
    {
        _itemService = itemService;
        _webHostEnvironment = webHostEnvironment;
    }

    // Tạo sản phẩm mới, đồng thời lưu trữ hình ảnh và tài liệu
    [HttpPost("create")]
    [Authorize]
    public IActionResult Create([FromForm] CreateItemWithFilesDto createItemWithFilesDto)
    {
        try
        {
            // Lấy ID người bán từ token
            var sellerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Lưu ảnh vào thư mục wwwroot/images và lưu đường dẫn vào cơ sở dữ liệu
            var imagePaths = new List<string>();
            if (createItemWithFilesDto.Images != null)
            {
                foreach (var image in createItemWithFilesDto.Images)
                {
                    var imageName = FileHelper.GenerateFileName(image.FileName);
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", imageName);

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    imagePaths.Add("/images/" + imageName); // Lưu đường dẫn cho cơ sở dữ liệu
                }
            }

            // Lưu tài liệu vào thư mục wwwroot/documents và lưu đường dẫn vào cơ sở dữ liệu
            var documentPaths = new List<string>();
            if (createItemWithFilesDto.Documents != null)
            {
                foreach (var document in createItemWithFilesDto.Documents)
                {
                    var documentName = FileHelper.GenerateFileName(document.FileName);
                    var documentPath = Path.Combine(_webHostEnvironment.WebRootPath, "documents", documentName);

                    using (var stream = new FileStream(documentPath, FileMode.Create))
                    {
                        document.CopyTo(stream);
                    }

                    documentPaths.Add("/documents/" + documentName); // Lưu đường dẫn cho cơ sở dữ liệu
                }
            }

            // Gọi service để lưu sản phẩm và liên kết hình ảnh, tài liệu
            var createdItem = _itemService.CreateItem(createItemWithFilesDto, sellerId, imagePaths, documentPaths);

            return Ok(new { Result = createdItem });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "Đã xảy ra lỗi: " + ex.Message });
        }
    }
}
