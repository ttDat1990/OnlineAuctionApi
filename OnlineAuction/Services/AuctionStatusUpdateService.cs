using Microsoft.EntityFrameworkCore;
using OnlineAuction.Models;

namespace OnlineAuction.Services;

public class AuctionStatusUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionStatusUpdateService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15); // Thời gian kiểm tra định kỳ

    public AuctionStatusUpdateService(IServiceProvider serviceProvider, ILogger<AuctionStatusUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //_logger.LogInformation("AuctionStatusUpdateService is starting.");

        //while (!stoppingToken.IsCancellationRequested)
        //{
        //    try
        //    {
        //        _logger.LogInformation("Checking for items that need to be activated or ended at {Time}.", DateTime.UtcNow);
        //        await UpdateBidStatuses();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while updating bid statuses.");
        //    }

        //    await Task.Delay(_interval, stoppingToken);
        //}

        //_logger.LogInformation("AuctionStatusUpdateService is stopping.");
    }

    private async Task UpdateBidStatuses()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var currentTime = DateTime.UtcNow;

            // Kích hoạt đấu giá với BidStartDate <= hiện tại và BidStatus là 'I' (Inactive)
            var itemsToActivate = dbContext.Items
                .Where(item => item.BidStartDate <= currentTime && item.BidStatus == "I")
                .ToList();

            if (itemsToActivate.Any())
            {
                _logger.LogInformation("Found {Count} items to activate.", itemsToActivate.Count);

                foreach (var item in itemsToActivate)
                {
                    _logger.LogInformation("Activating item with ItemId: {ItemId}, Title: {ItemTitle}.", item.ItemId, item.ItemTitle);
                    item.BidStatus = "A"; // Kích hoạt đấu giá
                }

                await dbContext.SaveChangesAsync();
                _logger.LogInformation("{Count} items have been activated.", itemsToActivate.Count);
            }
            else
            {
                _logger.LogInformation("No items found to activate at this time.");
            }

            // Kết thúc đấu giá với BidEndDate <= hiện tại và BidStatus là 'A' (Active)
            var itemsToEnd = dbContext.Items
                .Include(i => i.Bids) // Bao gồm các đấu thầu liên quan
                .Where(item => item.BidEndDate <= currentTime && item.BidStatus == "A")
                .ToList();

            if (itemsToEnd.Any())
            {
                _logger.LogInformation("Found {Count} items to end.", itemsToEnd.Count);

                foreach (var item in itemsToEnd)
                {
                    _logger.LogInformation("Ending auction for item with ItemId: {ItemId}, Title: {ItemTitle}.", item.ItemId, item.ItemTitle);
                    item.BidStatus = "E"; // Kết thúc đấu giá

                    // Xác định người đấu giá cuối cùng (người thắng)
                    var highestBid = item.Bids.OrderByDescending(b => b.BidAmount).FirstOrDefault();

                    if (highestBid != null)
                    {
                        // Tạo thông báo cho tất cả người tham gia đấu giá
                        var bidders = item.Bids.Select(b => b.BidderId).Distinct().ToList();

                        foreach (var bidderId in bidders)
                        {
                            var message = (bidderId == highestBid.BidderId)
                                ? $"You have won the auction for the item {item.ItemTitle}."
                                : $"You have lost the auction for the item {item.ItemTitle}.";

                            dbContext.Notifications.Add(new Notification
                            {
                                UserId = (int)bidderId,
                                Message = message,
                                IsRead = false,
                                NotificationDate = DateTime.Now
                            });
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"No bids placed for item {item.ItemId}");
                    }
                }

                await dbContext.SaveChangesAsync();
                _logger.LogInformation("{Count} items have been ended.", itemsToEnd.Count);
            }
            else
            {
                _logger.LogInformation("No items found to end at this time.");
            }
        }
    }

    public async Task TriggerUpdateBidStatuses()
    {
        await UpdateBidStatuses();
    }
}
