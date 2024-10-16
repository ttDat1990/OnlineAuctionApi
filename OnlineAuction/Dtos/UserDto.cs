﻿namespace OnlineAuction.Dtos;

public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public bool IsBlocked { get; set; }
    public int RatingScore { get; set; }
    public List<RatingDto>? RatingRatedUsers { get; set; } = new List<RatingDto>();
}

public class LoginRequestDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterUserDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class GoogleLoginRequestDto
{
    public string IdToken { get; set; } = null!;
}

public class ForgotPasswordDto
{
    public string Email { get; set; }
}

public class ResetPasswordDto
{
    public string Email { get; set; }
    public string ResetCode { get; set; }
    public string NewPassword { get; set; }
}

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Token { get; set; }
    public UserDto User { get; set; }
}




