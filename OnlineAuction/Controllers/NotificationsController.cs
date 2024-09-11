using Microsoft.AspNetCore.Mvc;
using OnlineAuction.Dtos;
using OnlineAuction.Services;

namespace OnlineAuction.Controllers;
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    public IActionResult CreateNotification([FromBody] CreateNotificationDto createNotificationDto)
    {
        var notificationId = _notificationService.CreateNotification(createNotificationDto);

        if (notificationId > 0)
        {
            return Ok(new { Message = "Notification created successfully", NotificationId = notificationId });
        }

        return BadRequest(new { Message = "Failed to create notification" });
    }

    [HttpPut("{id}/read")]
    public IActionResult MarkNotificationAsRead(int id)
    {
        var success = _notificationService.MarkNotificationAsRead(id);
        if (success)
        {
            return Ok(new { Message = "Notification marked as read." });
        }

        return NotFound(new { Message = "Notification not found or already read." });
    }
}
