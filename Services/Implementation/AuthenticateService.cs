using Common.Dto;
using Common.Models;
using Common.Params;
using Common.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.MyDbContext;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Services.Implementation;

public class AuthenticateService : IAuthenticateService
{
    private readonly IConfiguration _configuration;
    private readonly MyDbContext _context;

    public AuthenticateService(IConfiguration configuration, MyDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public async Task<ResultDTO> Register([FromBody] RegisterParams registerFrom)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            if (await _context.Authenticates.AnyAsync(u => u.UserName == registerFrom.UserName))
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Username has already been registered";
                return result;
            }

            var newUser = new Authenticate
            {
                UserName = registerFrom.UserName,
                Pw = BCrypt.Net.BCrypt.HashPassword(registerFrom.Password)
            };

            _context.Authenticates.Add(newUser);
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task<ResultDTO> Login([FromBody] LoginParams loginFrom)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var user = await _context.Authenticates.FirstOrDefaultAsync(u => u.UserName == loginFrom.UserName);
            if (user == null)
            {
                result.IsSuccess = false;
                result.Message = "User not found.";
                return result;
            }

            if (!BCrypt.Net.BCrypt.Verify(loginFrom.Password, user.Pw))
            {
                result.IsSuccess = false;
                result.Message = "Password is Invalid.";
                return result;
            }

            var accessToken = GenerateJwtToken(user.UserName, user.Id);
            var refreshToken = GenerateRefreshToken();

            // 更新資料庫中的 refreshToken
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            result.Data = new TokenViewModel()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task<ResultDTO> RefreshToken(string refreshToken)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var user = await _context.Authenticates.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (user == null || user.RefreshToken == null || user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                result.IsSuccess = false;
                result.Message = "Invalid refresh token.";
            }
            else
            {
                // 生成新的 JWT token 和 refreshToken
                var newAccessToken = GenerateJwtToken(user.UserName, user.Id);
                var newRefreshToken = GenerateRefreshToken();

                // 更新 refreshToken
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                result.Data = new TokenViewModel()
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                };
            }
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Message = ex.Message;
        }
        return result;
    }

    private string GenerateJwtToken(string userName, int userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtConfig:SecretKey"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim("UserName", userName),
                new Claim("UserId", userId.ToString()),
            }),
            Expires = DateTime.UtcNow.AddMinutes(30),
            Issuer = _configuration["JwtConfig:Issuer"], // 设置 Issuer
            Audience = _configuration["JwtConfig:Audience"], // 设置 Audience
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
