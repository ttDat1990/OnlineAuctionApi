using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuction.Dtos;
using OnlineAuction.Services;
using System.Security.Claims;

namespace OnlineAuction.Controllers;
[Route("api/[controller]")]
[ApiController]
public class BidsController : Controller
{
    private readonly IBidService _bidService;

    public BidsController(IBidService bidService)
    {
        _bidService = bidService;
    }

    [HttpPost]
    [Authorize(Roles = "NormalUser")]
    public IActionResult PlaceBid([FromBody] PlaceBidDto placeBidDto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var success = _bidService.PlaceBid(placeBidDto.ItemId, placeBidDto.BidAmount, userId);
            if (success)
            {
                return Ok(new { Message = "Bid placed successfully" });
            }
            return BadRequest(new { Message = "Failed to place bid" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{itemId}")]
    public IActionResult GetBidHistory(int itemId)
    {
        try
        {
            var bids = _bidService.GetBidHistory(itemId);

            if (bids != null)
            {
                return Ok(bids);
            }
            return NotFound(new { Message = "No bids found for this item." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "An error occurred: " + ex.Message });
        }
    }
}
