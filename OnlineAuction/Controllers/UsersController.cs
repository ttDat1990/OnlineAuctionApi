using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineAuction.Dtos;
using OnlineAuction.Services;

namespace OnlineAuction.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;
    private readonly IRatingService _ratingService;

    public UsersController(IUserService userService, INotificationService notificationService, IRatingService ratingService)
    {
        _userService = userService;
        _notificationService = notificationService;
        _ratingService = ratingService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
    {
        var result = await _userService.RegisterAsync(registerUserDto);
        if (!result)
        {
            return BadRequest("Username already exists.");
        }

        return Ok(new { Result = "success" });
    }

    [HttpPost("registerAdmin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterUserDto registerUserDto)
    {
        var result = await _userService.RegisterAdminAsync(registerUserDto);
        if (!result)
        {
            return BadRequest("Username already exists.");
        }

        return Ok(new { Result = "success" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
    {
        var response = await _userService.LoginAsync(loginRequest.Username, loginRequest.Password);
        if (response == null)
        {
            return Unauthorized("Username or password is incorrect.");
        }

        return Ok(response);
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        if (user == null)
        {
            return NotFound("User does not exist.");
        }

        return Ok(user);
    }

    [HttpPut("block/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BlockUser(int userId)
    {
        var result = await _userService.BlockUserAsync(userId);
        if (!result)
        {
            return NotFound("Người dùng không tồn tại.");
        }

        return Ok(new { Result = "success" });
    }

    [HttpGet]
    [Route("{userId}/notifications")]
    public IActionResult GetUserNotifications(int userId)
    {
        var notifications = _notificationService.GetUserNotifications(userId);
        if (notifications == null || notifications.Count == 0)
        {
            return NotFound(new { Message = "No notifications found for this user." });
        }

        return Ok(notifications);
    }

    // GET /api/users/{userId}/ratings
    [HttpGet("{userId}/ratings")]
    public async Task<IActionResult> GetUserRatings(int userId)
    {
        var userRatings = await _ratingService.GetUserRatings(userId);
        return Ok(userRatings);
    }
}
