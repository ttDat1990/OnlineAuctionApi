using OnlineAuction.Dtos;

namespace OnlineAuction.Services;

public interface INotificationService
{
    List<NotificationDto> GetUserNotifications(int userId);
    bool MarkNotificationAsRead(int notificationId);
    int CreateNotification(CreateNotificationDto createNotificationDto);
}
