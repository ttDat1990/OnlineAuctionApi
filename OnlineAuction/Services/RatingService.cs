using Microsoft.EntityFrameworkCore;
using OnlineAuction.Dtos;
using OnlineAuction.Models;

namespace OnlineAuction.Services;

public class RatingService : IRatingService
{
    private readonly DatabaseContext _dbContext;

    public RatingService(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CreateRating(RatingDto ratingDto)
    {
        var item = await _dbContext.Items
            .Include(i => i.Bids)
            .FirstOrDefaultAsync(i => i.ItemId == ratingDto.ItemId);

        if (item == null || item.BidStatus != "E")
        {
            throw new Exception("Auction is not yet completed or item does not exist.");
        }

        var highestBid = item.Bids.OrderByDescending(b => b.BidAmount).FirstOrDefault();
        if (highestBid == null || (highestBid.BidderId != ratingDto.RatedByUserId && ratingDto.RatedByUserId != item.SellerId))
        {
            throw new Exception("Only the seller and auction winner can rate each other.");
        }

        // Ensure that the rating does not already exist
        var existingRating = await _dbContext.Ratings
            .FirstOrDefaultAsync(r => r.ItemId == ratingDto.ItemId
                                      && ((r.RatedUserId == ratingDto.RatedUserId && r.RatedByUserId == ratingDto.RatedByUserId)
                                          || (r.RatedUserId == ratingDto.RatedByUserId && r.RatedByUserId == ratingDto.RatedUserId)));

        if (existingRating != null)
        {
            throw new Exception("A rating already exists between these users for this auction.");
        }

        // Create the new rating
        var rating = new Rating
        {
            RatedUserId = ratingDto.RatedUserId,
            RatedByUserId = ratingDto.RatedByUserId,
            ItemId = ratingDto.ItemId,
            RatingScore = ratingDto.RatingScore,
            Comments = ratingDto.Comments,
            RatingDate = DateTime.UtcNow
        };

        _dbContext.Ratings.Add(rating);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<List<Rating>> GetUserRatings(int userId)
    {
        return await _dbContext.Ratings
            .Where(r => r.RatedUserId == userId)
            .ToListAsync();
    }
}
