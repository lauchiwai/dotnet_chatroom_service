using Common.Helper.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Common.Helper.Implementation;

public class JwtUserInfo
{
    /// <summary>
    /// 使用者編號
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// 使用者名稱
    /// </summary>
    public string UserName { get; set; }
}

public class UserHelper : IUserHelper
{
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, string> _claimMappings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserHelper(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _claimMappings = new Dictionary<string, string>
        {
            { "UserId", "UserId" },
            { "UserName", "UserName" },
        };
    }

    /// <summary>
    /// 解析 JWT Token 並映射到強類型物件
    /// </summary>
    /// <typeparam name="T">目標類型</typeparam>
    /// <returns>映射後的物件</returns>
    public T ParseToken<T>()
        where T : new()
    {
        var token = GetTokenFromContext();
        var principal = ValidateToken(token);
        var claims = principal.Claims;

        var result = new T();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            var claimType = _claimMappings?.ContainsKey(property.Name) == true
                ? _claimMappings[property.Name]
                : property.Name;

            var claim = claims.FirstOrDefault(c => c.Type == claimType);
            if (claim != null)
            {
                var value = Convert.ChangeType(claim.Value, property.PropertyType);
                property.SetValue(result, value);
            }
        }

        return result;
    }

    /// <summary>
    /// 驗證 JWT Token 並返回 ClaimsPrincipal
    /// </summary>
    public ClaimsPrincipal ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtConfig:SecretKey"])),
            ValidateIssuer = false, 
            ValidateAudience = false, 
            ValidateLifetime = true, 
            ClockSkew = TimeSpan.Zero 
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch (SecurityTokenException ex)
        {
            throw new Exception("JWT Token 驗證失敗", ex);
        }
    }

    /// <summary>
    /// 從 HTTP 上下文中獲取 JWT Token
    /// </summary>
    private string GetTokenFromContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("HTTP 上下文不可用");
        }

        var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            throw new Exception("未提供有效的 JWT Token");
        }

        return authorizationHeader.Replace("Bearer ", string.Empty);
    }
}
