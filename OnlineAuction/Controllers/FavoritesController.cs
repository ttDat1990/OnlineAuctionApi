using Microsoft.AspNetCore.Mvc;
using OnlineAuction.Dtos;
using OnlineAuction.Services;
using System.Diagnostics;

namespace OnlineAuction.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FavoritesController : Controller
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    // POST /api/favorites/toggle
    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleFavorite([FromBody] ToggleFavoriteDto toggleFavoriteDto)
    {
        try
        {
            var result = await _favoriteService.ToggleFavorite(toggleFavoriteDto.UserId, toggleFavoriteDto.ItemId);
            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET /api/favorites/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserFavorites(int userId)
    {
        try
        {
            var favorites = await _favoriteService.GetUserFavorites(userId);
            if (favorites == null || !favorites.Any())
            {
                return NotFound(new { message = "No favorites found for this user." });
            }

            return Ok(favorites);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{userId}/isFavourite/{itemId}")]
    public IActionResult IsFavourite(int userId, int itemId)
    {
        Debug.WriteLine(userId);
        var isFavourite = _favoriteService.IsItemFavourite(userId, itemId);

        return Ok(new { IsFavourite = isFavourite });
    }

}
