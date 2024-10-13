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
        return Ok(response); // Trả về đối tượng thành công
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

    [HttpPost("loginWithGoogle")]
    public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginRequestDto googleLoginRequest)
    {
        var response = await _userService.LoginWithGoogleAsync(googleLoginRequest.IdToken);

        // Kiểm tra nếu response trả về là thông báo người dùng chưa đăng ký
        if (response is string && response.ToString() == "User not registered")
        {
            return BadRequest("User not registered. Please sign up first.");
        }

        if (response == null)
        {
            return Unauthorized("Google login failed.");
        }

        return Ok(response);
    }

    // POST: api/password/forgot
    [HttpPost("forgot")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        // Call service to generate reset code and send it via email
        var result = await _userService.GenerateResetCodeAsync(forgotPasswordDto.Email);
        if (!result)
        {
            return BadRequest(new { message = "Invalid or non-existent email address." });
        }

        return Ok(new { message = "A reset code has been sent to your email." });
    }

    // POST: api/password/reset
    [HttpPost("reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        // Call service to validate reset code and update the password
        var result = await _userService.ValidateResetCodeAsync(
            resetPasswordDto.Email,
            resetPasswordDto.ResetCode,
            resetPasswordDto.NewPassword
        );

        if (!result)
        {
            return BadRequest(new { message = "Invalid or expired reset code." });
        }

        return Ok(new { message = "Your password has been successfully updated." });
    }

    [Produces("application/json")]
    [HttpGet("showAllUsers")]
    public async Task<IActionResult> ShowAllUsers()
    {
        try
        {
            var users = await _userService.ShowAllUser();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Produces("application/json")]
    [HttpGet("findUser/{username}")]
    public async Task<IActionResult> FindUser(string username)
    {
        try
        {
            var users = await _userService.FindUsers(username);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpPatch("StatusUser/{username}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> StatusUSer(string username)
    {
        try
        {
            var userC = await _userService.BlockAndUnBlock(username);
            return Ok(userC);

        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}
