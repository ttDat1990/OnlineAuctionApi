using Microsoft.AspNetCore.Mvc;
using OnlineAuction.Dtos;
using OnlineAuction.Services;

namespace OnlineAuction.Controllers;
[ApiController]
[Route("api/[controller]")]
public class RatingsController : Controller
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    // POST /api/ratings
    [HttpPost]
    public async Task<IActionResult> CreateRating(RatingDto ratingDto)
    {
        try
        {
            var result = await _ratingService.CreateRating(ratingDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET /api/ratings/item/{itemId}
    [HttpGet("item/{itemId}")]
    public async Task<IActionResult> GetRatingsByItemId(int itemId)
    {
        var ratings = await _ratingService.GetRatingsByItemId(itemId);

        // Nếu không có ratings, trả về một mảng rỗng
        return Ok(ratings ?? new List<RatingDto>());
    }
}
