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
    private readonly IRepository<Authenticate> _authRepository; 

    public AuthenticateService(IConfiguration configuration, MyDbContext context, IRepository<Authenticate> authRepository)
    {
        _configuration = configuration;
        _context = context;
        _authRepository = authRepository;
    }

    public async Task<ResultDTO> RamdomRegister()
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            var randomRegisterFrom = new RegisterParams
            {
                Username = Guid.NewGuid().ToString(),
                Password = "AdminPw006*"
            };

            if (await _authRepository.ExistsAsync(u => u.UserName == randomRegisterFrom.Username))
            {
                result.IsSuccess = false;
                result.Message = "Username has already been registered";
                return result;
            }

            var newUser = new Authenticate
            {
                UserName = randomRegisterFrom.Username,
                Pw = BCrypt.Net.BCrypt.HashPassword(randomRegisterFrom.Password)
            };

            await _authRepository.AddAsync(newUser);
            await _authRepository.SaveChangesAsync();

            result.Data = randomRegisterFrom;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task<ResultDTO> Register([FromBody] RegisterParams registerFrom)
    {
        var result = new ResultDTO() { IsSuccess = true };
        try
        {
            if (await _authRepository.ExistsAsync(u => u.UserName == registerFrom.Username))
            {
                result.IsSuccess = false;
                result.Message = "Username has already been registered";
                return result;
            }

            var newUser = new Authenticate
            {
                UserName = registerFrom.Username,
                Pw = BCrypt.Net.BCrypt.HashPassword(registerFrom.Password)
            };

            await _authRepository.AddAsync(newUser);
            await _authRepository.SaveChangesAsync();
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
            var user = await _authRepository.GetQueryable()
                .FirstOrDefaultAsync(u => u.UserName == loginFrom.Username);

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

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            _authRepository.Update(user);
            await _authRepository.SaveChangesAsync();

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
            var user = await _authRepository.GetQueryable()
               .SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshToken == null || user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                result.IsSuccess = false;
                result.Message = "Invalid refresh token.";
            }
            else
            {
                var newAccessToken = GenerateJwtToken(user.UserName, user.Id);
                var newRefreshToken = GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                _authRepository.Update(user);
                await _authRepository.SaveChangesAsync();

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
        var key = Encoding.UTF8.GetBytes(_configuration["JwtConfig:SecretKey"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim("UserName", userName),
                new Claim("UserId", userId.ToString()),
            }),
            Expires = DateTime.UtcNow.AddMinutes(30),
            Issuer = _configuration["JwtConfig:Issuer"],
            Audience = _configuration["JwtConfig:Audience"],
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
