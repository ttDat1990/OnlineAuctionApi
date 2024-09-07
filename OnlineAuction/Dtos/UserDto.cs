namespace OnlineAuction.Dtos;

public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public int RatingScore { get; set; }
}
