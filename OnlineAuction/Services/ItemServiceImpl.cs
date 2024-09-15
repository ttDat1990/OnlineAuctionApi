using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineAuction.Dtos;
using OnlineAuction.Models;

namespace OnlineAuction.Services;

public class ItemServiceImpl : IItemService
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public ItemServiceImpl(DatabaseContext dbContext, IMapper mapper, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _configuration = configuration;
    }
    public ItemDto? GetItemById(int id)
    {
        var item = _dbContext.Items
        .Include(i => i.Images)     // Bao gồm các hình ảnh liên quan
        .Include(i => i.Documents)  // Bao gồm các tài liệu liên quan
        .FirstOrDefault(i => i.ItemId == id);

        if (item == null)
        {
            return null; // Không tìm thấy sản phẩm
        }

        var itemDto = _mapper.Map<ItemDto>(item);

        return itemDto;
    }

    public bool CreateItem(CreateItemWithFilesDto createItemWithFilesDto, int sellerId, List<string> imagePaths, List<string> documentPaths)
    {
        //var item = new Item
        //{
        //    ItemTitle = createItemWithFilesDto.ItemTitle,
        //    ItemDescription = createItemWithFilesDto.ItemDescription,
        //    MinimumBid = createItemWithFilesDto.MinimumBid,
        //    CurrentBid = createItemWithFilesDto.MinimumBid,
        //    BidIncrement = createItemWithFilesDto.BidIncrement,
        //    BidStartDate = createItemWithFilesDto.BidStartDate,
        //    BidEndDate = createItemWithFilesDto.BidEndDate,
        //    CategoryId = createItemWithFilesDto.CategoryId,
        //    SellerId = sellerId,
        //    BidStatus = "I",
        //};

        // Sử dụng AutoMapper để ánh xạ từ DTO đến Entity
        var item = _mapper.Map<Item>(createItemWithFilesDto);

        // Cập nhật các thuộc tính khác
        item.SellerId = sellerId;
        item.BidStatus = "I";

        // Lưu sản phẩm vào cơ sở dữ liệu
        _dbContext.Items.Add(item);
        _dbContext.SaveChanges();

        // Lưu các đường dẫn hình ảnh vào bảng Images
        if (imagePaths != null && imagePaths.Count > 0)
        {
            foreach (var imagePath in imagePaths)
            {
                var image = new Image
                {
                    ImageUrl = imagePath,
                    ItemId = item.ItemId
                };
                _dbContext.Images.Add(image);
            }
        }

        // Lưu các đường dẫn tài liệu vào bảng Documents
        if (documentPaths != null && documentPaths.Count > 0)
        {
            foreach (var documentPath in documentPaths)
            {
                var document = new Document
                {
                    DocumentUrl = documentPath,
                    ItemId = item.ItemId
                };
                _dbContext.Documents.Add(document);
            }
        }

        return _dbContext.SaveChanges() > 0;
    }

    public bool UpdateItem(int id, UpdateItemWithFilesDto updateItemWithFilesDto, int sellerId, List<string> imagePaths, List<string> documentPaths)
    {
        var existingItem = _dbContext.Items.Find(id);
        if (existingItem == null)
        {
            throw new Exception("Item is not exists.");
        }

        // Kiểm tra quyền sở hữu sản phẩm
        if (existingItem.SellerId != sellerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this product.");
        }

        // Chỉ cập nhật số tiền tăng giá đấu thầu
        existingItem.BidIncrement = updateItemWithFilesDto.BidIncrement;

        // Nếu có hình ảnh mới, xóa các hình ảnh cũ và thêm hình ảnh mới
        if (imagePaths != null && imagePaths.Count > 0)
        {
            var oldImages = _dbContext.Images.Where(img => img.ItemId == existingItem.ItemId).ToList();
            _dbContext.Images.RemoveRange(oldImages);

            foreach (var imagePath in imagePaths)
            {
                var image = new Image
                {
                    ImageUrl = imagePath,
                    ItemId = existingItem.ItemId
                };
                _dbContext.Images.Add(image);
            }
        }

        // Nếu có tài liệu mới, xóa các tài liệu cũ và thêm tài liệu mới
        if (documentPaths != null && documentPaths.Count > 0)
        {
            var oldDocuments = _dbContext.Documents.Where(doc => doc.ItemId == existingItem.ItemId).ToList();
            _dbContext.Documents.RemoveRange(oldDocuments);

            foreach (var documentPath in documentPaths)
            {
                var document = new Document
                {
                    DocumentUrl = documentPath,
                    ItemId = existingItem.ItemId
                };
                _dbContext.Documents.Add(document);
            }
        }

        return _dbContext.SaveChanges() > 0;
    }

    public bool DeleteItem(int itemId)
    {
        var item = _dbContext.Items.Find(itemId);
        if (item == null) return false;

        // Xóa các hình ảnh và tài liệu liên quan
        var images = _dbContext.Images.Where(i => i.ItemId == itemId).ToList();
        var documents = _dbContext.Documents.Where(d => d.ItemId == itemId).ToList();

        _dbContext.Images.RemoveRange(images);
        _dbContext.Documents.RemoveRange(documents);

        // Xóa sản phẩm
        _dbContext.Items.Remove(item);
        _dbContext.SaveChanges();

        return true;
    }

    public List<ItemDto> GetItemsByCategory(int categoryId)
    {
        // Lấy tất cả các sản phẩm theo CategoryId
        var items = _dbContext.Items
            .Where(i => i.CategoryId == categoryId)
            .Include(i => i.Images)     // Bao gồm cả các hình ảnh liên quan
            .Include(i => i.Documents)  // Bao gồm cả các tài liệu liên quan
            .ToList();

        var itemDtos = _mapper.Map<List<ItemDto>>(items);

        return itemDtos;
    }

    public List<ItemDto> SearchItems(string query, int? categoryId = null, int? sellerId = null, string bidStatus = null, DateTime? bidStartDate = null, DateTime? bidEndDate = null)
    {
        // Chuyển đổi từ khóa tìm kiếm thành chữ thường
        var lowerQuery = query?.ToLower();

        // Bắt đầu truy vấn từ bảng Items
        var itemsQuery = _dbContext.Items
            .Include(i => i.Images)     // Bao gồm các hình ảnh liên quan
            .Include(i => i.Documents)  // Bao gồm các tài liệu liên quan
            .AsQueryable();

        // Nếu có từ khóa tìm kiếm, lọc theo tiêu đề hoặc mô tả sản phẩm
        if (!string.IsNullOrEmpty(lowerQuery))
        {
            itemsQuery = itemsQuery.Where(i => i.ItemTitle.ToLower().Contains(lowerQuery) ||
                                               i.ItemDescription.ToLower().Contains(lowerQuery));
        }

        // Lọc theo CategoryId nếu có
        if (categoryId.HasValue)
        {
            itemsQuery = itemsQuery.Where(i => i.CategoryId == categoryId.Value);
        }

        // Lọc theo SellerId nếu có
        if (sellerId.HasValue)
        {
            itemsQuery = itemsQuery.Where(i => i.SellerId == sellerId.Value);
        }

        // Lọc theo BidStatus nếu có
        if (!string.IsNullOrEmpty(bidStatus))
        {
            itemsQuery = itemsQuery.Where(i => i.BidStatus == bidStatus);
        }

        // Lọc theo BidStartDate nếu có
        if (bidStartDate.HasValue)
        {
            itemsQuery = itemsQuery.Where(i => i.BidStartDate >= bidStartDate.Value);
        }

        // Lọc theo BidEndDate nếu có
        if (bidEndDate.HasValue)
        {
            itemsQuery = itemsQuery.Where(i => i.BidEndDate <= bidEndDate.Value);
        }

        // Thực hiện truy vấn
        var items = itemsQuery.ToList();

        // Map các đối tượng Item sang ItemDto, bao gồm hình ảnh và tài liệu
        var itemDtos = _mapper.Map<List<ItemDto>>(items);

        return itemDtos;
    }

    public List<ItemDto> GetAllItems()
    {
        var items = _dbContext.Items
            .Include(i => i.Images)     // Include related images
            .Include(i => i.Documents)  // Include related documents
            .ToList();

        var itemDtos = _mapper.Map<List<ItemDto>>(items);
        return itemDtos;
    }



}
