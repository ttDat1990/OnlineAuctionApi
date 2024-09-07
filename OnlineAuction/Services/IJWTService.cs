using OnlineAuction.Models;

namespace OnlineAuction.Services;

public interface IJWTService
{
    string GenerateToken(User user);
}
