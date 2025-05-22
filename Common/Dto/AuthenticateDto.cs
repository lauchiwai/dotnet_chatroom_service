using System.ComponentModel.DataAnnotations;

namespace Common.Dto;

public class AuthenticateDto
{
    /// <summary>
    /// 使用者賬號
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = null!;

    /// <summary>
    /// pw
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 6)]
    public string Password { get; set; } = null!;
}
