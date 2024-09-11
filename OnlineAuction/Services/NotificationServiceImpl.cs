using OnlineAuction.Dtos;
using OnlineAuction.Models;

namespace OnlineAuction.Services;

public class NotificationServiceImpl : INotificationService
{
    private readonly DatabaseContext _dbContext;

    public NotificationServiceImpl(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }
    public List<NotificationDto> GetUserNotifications(int userId)
    {
        var notifications = _dbContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.NotificationDate)
            .Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                Message = n.Message,
                IsRead = n.IsRead,
                NotificationDate = n.NotificationDate
            })
            .ToList();

        return notifications;
    }
    public bool MarkNotificationAsRead(int notificationId)
    {
        var notification = _dbContext.Notifications.FirstOrDefault(n => n.NotificationId == notificationId);

        if (notification == null || notification.IsRead)
        {
            return false;
        }

        notification.IsRead = true;
        _dbContext.SaveChanges();
        return true;
    }

    public int CreateNotification(CreateNotificationDto createNotificationDto)
    {
        var notification = new Notification
        {
            UserId = createNotificationDto.UserId,
            Message = createNotificationDto.Message,
            NotificationDate = DateTime.Now,
            IsRead = false
        };

        _dbContext.Notifications.Add(notification);
        _dbContext.SaveChanges();

        return notification.NotificationId;
    }
}
