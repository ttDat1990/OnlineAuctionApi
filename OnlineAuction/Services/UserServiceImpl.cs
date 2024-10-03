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

    public UserServiceImpl(DatabaseContext dbContext, IMapper mapper, IJWTService jWTService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _jWTService = jWTService;
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
}
