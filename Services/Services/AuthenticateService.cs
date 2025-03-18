using Common.Dto;
using Common.Models;
using Common.Params;
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

namespace Services.Services;

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
}
