using OnlineAuction.Dtos;

namespace OnlineAuction.Services;

public interface IBidService
{
    bool PlaceBid(int itemId, decimal bidAmount, int bidderId);
    IEnumerable<BidDto> GetBidHistory(int itemId);
}
