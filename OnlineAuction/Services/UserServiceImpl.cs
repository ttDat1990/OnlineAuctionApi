using AutoMapper;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using OnlineAuction.Dtos;
using OnlineAuction.Models;

namespace OnlineAuction.Services;

public class UserServiceImpl : IUserService
{
    private readonly DatabaseContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IJWTService _jWTService;
    private readonly IEmailService _emailService;

    public UserServiceImpl(DatabaseContext dbContext, IMapper mapper, IJWTService jWTService, IEmailService emailService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _jWTService = jWTService;
        _emailService = emailService;
    }

    public async Task<bool> RegisterAsync(RegisterUserDto registerUserDto)
    {
        if (_dbContext.Users.Any(u => u.Username == registerUserDto.Username))
        {
            return false;
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password);
        var user = new User
        {
            Username = registerUserDto.Username,
            Email = registerUserDto.Email,
            Password = hashedPassword,
            Role = "NormalUser",
            CreatedDate = DateTime.Now,
            RatingScore = 0,
            IsBlocked = false
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<object?> LoginAsync(string username, string password)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return null;
        }

        var token = _jWTService.GenerateToken(user);

        var userDto = _mapper.Map<UserDto>(user);

        return new
        {
            Token = token,
            User = userDto
        };
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _dbContext.Users
            .Include(u => u.RatingRatedByUsers)   // Chỉ cần Include quan hệ này
            .SingleOrDefaultAsync(u => u.Username == username);

        if (user != null)
        {
            // Nạp thêm dữ liệu cho các RatingRatedByUsers
            foreach (var rating in user.RatingRatedByUsers)
            {
                await _dbContext.Entry(rating)
                    .Reference(r => r.RatedByUser) // Chỉ cần nạp thêm RatedByUser nếu cần
                    .LoadAsync();
            }
        }

        return user == null ? null : _mapper.Map<UserDto>(user);
    }


    public async Task<bool> BlockUserAsync(int userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return false;

        user.IsBlocked = true;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RegisterAdminAsync(RegisterUserDto registerUserDto)
    {
        if (_dbContext.Users.Any(u => u.Username == registerUserDto.Username))
        {
            return false;
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password);
        var user = new User
        {
            Username = registerUserDto.Username,
            Email = registerUserDto.Email,
            Password = hashedPassword,
            Role = "Admin",
            CreatedDate = DateTime.Now,
            RatingScore = 0,
            IsBlocked = false
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<object?> LoginWithGoogleAsync(string idToken)
    {
        GoogleJsonWebSignature.Payload payload;

        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
        }
        catch (Exception)
        {
            return null;
        }

        // Kiểm tra xem người dùng đã tồn tại hay chưa
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == payload.Email);

        // Nếu người dùng chưa tồn tại
        if (user == null)
        {
            return "User not registered";  // Thông báo người dùng chưa đăng ký
        }

        var token = _jWTService.GenerateToken(user);
        var userDto = _mapper.Map<UserDto>(user);

        return new
        {
            Token = token,
            User = userDto
        };
    }

    public async Task<bool> GenerateResetCodeAsync(string email)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;

        // Tạo mã reset gồm 6 chữ số
        var resetCode = new Random().Next(100000, 999999).ToString();

        // Cập nhật mã reset và thời gian hết hạn
        user.ResetCode = resetCode;
        user.ResetCodeExpiration = DateTime.Now.AddMinutes(15); // Mã có hiệu lực trong 15 phút
        await _dbContext.SaveChangesAsync();

        // Gửi email chứa mã reset
        await _emailService.SendEmailAsync(user.Email, "Password Reset Code", $"Your reset code is: {resetCode}");

        return true;
    }

    public async Task<bool> ValidateResetCodeAsync(string email, string resetCode, string newPassword)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user == null || user.ResetCode != resetCode || user.ResetCodeExpiration < DateTime.Now)
        {
            return false; // Mã không hợp lệ hoặc đã hết hạn
        }

        // Cập nhật mật khẩu mới và xóa mã reset
        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ResetCode = null;
        user.ResetCodeExpiration = null;
        await _dbContext.SaveChangesAsync();

        return true;
    }

}
