using AutoMapper;
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

    public ItemDto CreateItem(CreateItemWithFilesDto createItemWithFilesDto, int sellerId, List<string> imagePaths, List<string> documentPaths)
    {
        var item = new Item
        {
            ItemTitle = createItemWithFilesDto.ItemTitle,
            ItemDescription = createItemWithFilesDto.ItemDescription,
            MinimumBid = createItemWithFilesDto.MinimumBid,
            CurrentBid = createItemWithFilesDto.MinimumBid,
            BidIncrement = createItemWithFilesDto.BidIncrement,
            BidStartDate = createItemWithFilesDto.BidStartDate,
            BidEndDate = createItemWithFilesDto.BidEndDate,
            CategoryId = createItemWithFilesDto.CategoryId,
            SellerId = sellerId,
            BidStatus = "I",
        };

        // Lưu sản phẩm vào cơ sở dữ liệu
        _dbContext.Items.Add(item);
        _dbContext.SaveChanges();

        // Lưu các đường dẫn hình ảnh vào bảng Images
        foreach (var imagePath in imagePaths)
        {
            var image = new Image
            {
                ImageUrl = imagePath,
                ItemId = item.ItemId
            };
            _dbContext.Images.Add(image);
        }

        // Lưu các đường dẫn tài liệu vào bảng Documents
        foreach (var documentPath in documentPaths)
        {
            var document = new Document
            {
                DocumentUrl = documentPath,
                ItemId = item.ItemId
            };
            _dbContext.Documents.Add(document);
        }

        _dbContext.SaveChanges();

        List<string> imageFullPaths = imagePaths.Select(item => _configuration["Url"] + item).ToList();
        List<string> documentFullPaths = documentPaths.Select(item => _configuration["Url"] + item).ToList();

        ItemDto itemDto = new ItemDto
        {
            ItemId = item.ItemId,
            ItemTitle = item.ItemTitle,
            ItemDescription = item.ItemDescription,
            MinimumBid = item.MinimumBid,
            CurrentBid = (decimal)item.CurrentBid,
            BidIncrement = item.BidIncrement,
            BidStartDate = item.BidStartDate,
            BidEndDate = item.BidEndDate,
            CategoryId = (int)item.CategoryId,
            Images = imageFullPaths,
            Documents = documentFullPaths
        };

        return itemDto;
    }
}
