namespace OnlineAuction.Dtos;

public class NotificationDto
{
    public int NotificationId { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime NotificationDate { get; set; }
}

public class CreateNotificationDto
{
    public int UserId { get; set; }
    public string Message { get; set; }
}
