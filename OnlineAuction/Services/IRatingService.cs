using OnlineAuction.Dtos;
using OnlineAuction.Models;

namespace OnlineAuction.Services;

public interface IRatingService
{
    Task<bool> CreateRating(RatingDto ratingDto);
    Task<List<Rating>> GetUserRatings(int userId);
}
