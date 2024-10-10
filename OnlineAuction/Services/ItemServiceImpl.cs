using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineAuction.Dtos;
using OnlineAuction.Models;
using System.Diagnostics;

namespace OnlineAuction.Services;

public class ItemServiceImpl : IItemService
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly INotificationService _notificationService;

    public ItemServiceImpl(DatabaseContext dbContext, IMapper mapper, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
        _notificationService = notificationService;
    }
    public ItemDto? GetItemById(int id)
    {
        var item = _dbContext.Items
        .Include(i => i.Images)     // Bao gồm các hình ảnh liên quan
        .Include(i => i.Documents)  // Bao gồm các tài liệu liên quan
        .Include(i => i.Favorites)
        .FirstOrDefault(i => i.ItemId == id);

        if (item == null)
        {
            return null; // Không tìm thấy sản phẩm
        }

        // Tính số lượng người yêu thích sản phẩm
        var favoritesCount = item.Favorites.Count(f => f.Status);

        // Sử dụng AutoMapper để map từ entity sang DTO
        var itemDto = _mapper.Map<ItemDto>(item);

        // Gán số lượng yêu thích vào DTO
        itemDto.FavoritesCount = favoritesCount;

        return itemDto;
    }

    public bool CreateItem(CreateItemWithFilesDto createItemWithFilesDto, int sellerId, List<string> imagePaths, List<string> documentPaths)
    {

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
        _dbContext.SaveChanges();

        // Lên lịch cập nhật trạng thái đấu giá dựa trên BidStartDate và BidEndDate
        ScheduleBidStatusUpdate(item);

        return true;
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

    public async Task<bool> DeleteItem(int itemId, string description)
    {
        var item = await _dbContext.Items.FindAsync(itemId);
        if (item == null) throw new Exception("Item does not exist.");

        // Xóa các hình ảnh và tài liệu liên quan
        var images = await _dbContext.Images.Where(i => i.ItemId == itemId).ToListAsync();
        var documents = await _dbContext.Documents.Where(d => d.ItemId == itemId).ToListAsync();

        _dbContext.Images.RemoveRange(images);
        _dbContext.Documents.RemoveRange(documents);

        // Xóa sản phẩm
        _dbContext.Items.Remove(item);
        await _dbContext.SaveChangesAsync();

        var createNotificationDto = new CreateNotificationDto()
        {
            UserId = (int)item.SellerId,
            Message = description,
        };
        _notificationService.CreateNotification(createNotificationDto);


        return true;
    }


    public List<ItemDto> GetItemsByCategory(int categoryId)
    {
        // Lấy tất cả các sản phẩm theo CategoryId
        var items = _dbContext.Items
            .Where(i => i.CategoryId == categoryId)
            .Include(i => i.Images)     // Bao gồm các hình ảnh liên quan
            .Include(i => i.Documents)  // Bao gồm các tài liệu liên quan
            .Include(i => i.Favorites)  // Bao gồm cả yêu thích
            .ToList();

        // Map các đối tượng Item sang ItemDto
        var itemDtos = _mapper.Map<List<ItemDto>>(items);

        // Cập nhật số lượng yêu thích cho mỗi ItemDto
        foreach (var itemDto in itemDtos)
        {
            var item = items.First(i => i.ItemId == itemDto.ItemId);
            itemDto.FavoritesCount = item.Favorites.Count(f => f.Status);
        }

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
            .Include(i => i.Favorites)  // Bao gồm cả yêu thích
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

        // Map các đối tượng Item sang ItemDto
        var itemDtos = _mapper.Map<List<ItemDto>>(items);

        // Cập nhật số lượng yêu thích cho mỗi ItemDto
        foreach (var itemDto in itemDtos)
        {
            var item = items.First(i => i.ItemId == itemDto.ItemId);
            itemDto.FavoritesCount = item.Favorites.Count(f => f.Status);
        }

        return itemDtos;
    }


    public List<ItemDto> GetAllItems()
    {
        var items = _dbContext.Items
            .Include(i => i.Images)     // Bao gồm các hình ảnh liên quan
            .Include(i => i.Documents)  // Bao gồm các tài liệu liên quan
            .Include(i => i.Favorites)  // Bao gồm cả yêu thích
            .ToList();

        // Map các đối tượng Item sang ItemDto
        var itemDtos = _mapper.Map<List<ItemDto>>(items);

        // Cập nhật số lượng yêu thích cho mỗi ItemDto
        foreach (var itemDto in itemDtos)
        {
            var item = items.First(i => i.ItemId == itemDto.ItemId);
            itemDto.FavoritesCount = item.Favorites.Count(f => f.Status);
        }

        return itemDtos;
    }


    // Phương thức lên lịch cập nhật trạng thái BidStatus
    private async void ScheduleBidStatusUpdate(Item item)
    {
        // Tính toán thời gian đến BidStartDate và BidEndDate
        var currentTime = DateTime.Now;
        var startDelay = item.BidStartDate - currentTime;
        var endDelay = item.BidEndDate - item.BidStartDate;
        Debug.WriteLine("startDelay :" + startDelay);
        Debug.WriteLine("endDelay :" + endDelay);

        // Theo dõi thời gian delay và ghi log liên tục
        await TrackDelay(startDelay, "BidStartDate", item.ItemId);

        // Lên lịch kích hoạt đấu giá (BidStatus = 'A')
        if (startDelay.TotalMilliseconds > 0)
        {
            UpdateBidStatus(item.ItemId, "A");
        }

        // Theo dõi thời gian delay và ghi log liên tục cho BidEndDate
        await TrackDelay(endDelay, "BidEndDate", item.ItemId);

        // Lên lịch kết thúc đấu giá (BidStatus = 'E')
        if (endDelay.TotalMilliseconds > 0)
        {
            UpdateBidStatus(item.ItemId, "E");
        }
    }

    private async Task TrackDelay(TimeSpan delay, string eventType, int itemId)
    {
        // Nếu thời gian delay nhỏ hơn hoặc bằng 0, không làm gì
        if (delay.TotalMilliseconds <= 0)
        {
            return;
        }

        var remainingTime = delay;

        while (remainingTime.TotalMilliseconds > 0)
        {
            Debug.WriteLine($"[{DateTime.Now}] Remaining time for {eventType} of item {itemId}: {remainingTime}");

            // Chờ một khoảng thời gian nhỏ (ví dụ: 5s) trước khi ghi log lần tiếp theo
            var interval = TimeSpan.FromSeconds(5);
            var waitTime = remainingTime > interval ? interval : remainingTime;

            // Chờ cho đến khi interval hoặc hết thời gian
            await Task.Delay(waitTime);

            // Tính toán lại thời gian còn lại
            remainingTime -= waitTime;
        }

        Debug.WriteLine($"[{DateTime.Now}] Event {eventType} for item {itemId} is triggered.");
    }

    // Phương thức cập nhật trạng thái đấu giá
    private void UpdateBidStatus(int itemId, string newStatus)
    {
        // Tạo một scope mới để lấy DbContext từ DI
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var item = dbContext.Items
                .Include(i => i.Bids) // Bao gồm các bids
                .FirstOrDefault(i => i.ItemId == itemId);

            if (item != null)
            {
                // Thay đổi trạng thái đấu giá
                item.BidStatus = newStatus;
                dbContext.SaveChanges();

                // Nếu chuyển trạng thái từ "I" sang "A" (Đấu giá bắt đầu)
                if (newStatus == "A")
                {
                    Debug.WriteLine("Status T -> A");
                    // Gửi thông báo cho người bán rằng đấu giá đã bắt đầu
                    dbContext.Notifications.Add(new Notification
                    {
                        UserId = (int)item.SellerId,
                        Message = $"Your auction for the item '{item.ItemTitle}'(id:{item.ItemId}) has started.",
                        IsRead = false,
                        NotificationDate = DateTime.Now
                    });
                    dbContext.SaveChanges();
                }

                // Nếu chuyển trạng thái từ "A" sang "E" (Đấu giá kết thúc)
                if (newStatus == "E")
                {
                    Debug.WriteLine("Status A -> E : 1");
                    // Tìm bid thắng cuộc nếu có
                    var highestBid = item.Bids.OrderByDescending(b => b.BidAmount).FirstOrDefault();

                    if (highestBid != null)
                    {
                        Debug.WriteLine("Status A -> E : 2");
                        // Gửi thông báo cho người thắng đấu giá
                        dbContext.Notifications.Add(new Notification
                        {
                            UserId = (int)highestBid.BidderId,
                            Message = $"Congratulations! You won the auction for the item '{item.ItemTitle}'(id:{item.ItemId}).",
                            IsRead = false,
                            NotificationDate = DateTime.Now
                        });

                        // Gửi thông báo cho tất cả những người thua cuộc
                        var losingBidders = item.Bids
                            .Where(b => b.BidderId != highestBid.BidderId)
                            .Select(b => b.BidderId)
                            .Distinct()
                            .ToList();
                        Debug.WriteLine("Status A -> E : 3");

                        foreach (var loserId in losingBidders)
                        {
                            dbContext.Notifications.Add(new Notification
                            {
                                UserId = (int)loserId,
                                Message = $"You lost the auction for the item '{item.ItemTitle}'(id:{item.ItemId}).",
                                IsRead = false,
                                NotificationDate = DateTime.Now
                            });
                        }

                        // Gửi thông báo cho người bán về kết quả đấu giá
                        Debug.WriteLine("Status A -> E : 4");
                        dbContext.Notifications.Add(new Notification
                        {
                            UserId = (int)item.SellerId,
                            Message = $"The auction for your item '{item.ItemTitle}'(id:{item.ItemId}) has ended. The winner is '{highestBid.Bidder.Username}' with a bid of {highestBid.BidAmount}.",
                            IsRead = false,
                            NotificationDate = DateTime.Now
                        });
                    }
                    else
                    {
                        Debug.WriteLine("Status A -> E : 5");
                        // Nếu không có ai đấu giá, gửi thông báo cho người bán
                        dbContext.Notifications.Add(new Notification
                        {
                            UserId = (int)item.SellerId,
                            Message = $"The auction for your item '{item.ItemTitle}'(id:{item.ItemId}) has ended with no bids.",
                            IsRead = false,
                            NotificationDate = DateTime.Now
                        });
                    }

                    dbContext.SaveChanges();
                }
            }
        }
    }


}
