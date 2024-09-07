using Microsoft.IdentityModel.Tokens;
using OnlineAuction.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineAuction.Services;

public class JWTServiceImpl : IJWTService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JWTServiceImpl(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"];
        _issuer = configuration["Jwt:Issuer"];
        _audience = configuration["Jwt:Audience"];
    }

    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _issuer,
            Audience = _audience
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
