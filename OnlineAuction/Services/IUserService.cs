using OnlineAuction.Dtos;

namespace OnlineAuction.Services;

public interface IUserService
{
    public Task<List<UserDto>> ShowAllUser();
    public Task<List<UserDto>> FindUsers(string username);
    Task<bool> RegisterAsync(RegisterUserDto registerUserDto);
    Task<bool> RegisterAdminAsync(RegisterUserDto registerUserDto);
    Task<LoginResponseDto> LoginAsync(string email, string password);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<bool> BlockUserAsync(int userId);
    public Task<bool> BlockAndUnBlock(string username);
    public Task<object?> LoginWithGoogleAsync(string idToken);
    public Task<bool> GenerateResetCodeAsync(string email);
    public Task<bool> ValidateResetCodeAsync(string email, string resetCode, string newPassword);
}
