using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineAuction.Dtos;
using OnlineAuction.Models;

namespace OnlineAuction.Services;

public class BidServiceImpl : IBidService
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;

    public BidServiceImpl(DatabaseContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public bool PlaceBid(int itemId, decimal bidAmount, int bidderId)
    {
        var item = _dbContext.Items
            .Include(i => i.Bids)
            .FirstOrDefault(i => i.ItemId == itemId);

        if (item == null)
        {
            throw new ArgumentException("Item not found");
        }

        // Kiểm tra trạng thái đấu thầu của sản phẩm
        if (item.BidStatus != "A")
        {
            throw new InvalidOperationException("Bidding is not allowed for inactive items");
        }

        // Kiểm tra giá thầu phải lớn hơn giá hiện tại
        if (bidAmount <= item.CurrentBid)
        {
            throw new InvalidOperationException("Bid amount must be greater than the current bid");
        }

        // Kiểm tra giá thầu phải chênh lệch với giá hiện tại chia hết cho BidIncrement
        if ((bidAmount - item.CurrentBid) % item.BidIncrement != 0)
        {
            throw new InvalidOperationException("Bid amount must be a multiple of the bid increment");
        }

        var bid = new Bid
        {
            ItemId = itemId,
            BidderId = bidderId,
            BidAmount = bidAmount,
            BidDate = DateTime.UtcNow
        };

        item.CurrentBid = bidAmount;
        _dbContext.Bids.Add(bid);

        return _dbContext.SaveChanges() > 0;
    }

    public IEnumerable<BidDto> GetBidHistory(int itemId)
    {
        var bids = _dbContext.Bids
            .Where(b => b.ItemId == itemId)
            .Include(b => b.Bidder)
            .OrderByDescending(b => b.BidDate)
            .ToList();

        return _mapper.Map<IEnumerable<BidDto>>(bids);
    }
}
