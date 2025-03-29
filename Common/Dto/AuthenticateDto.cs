using System.ComponentModel.DataAnnotations;

namespace Common.Dto;

public class AuthenticateDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = null!;

    [Required]
    [StringLength(50, MinimumLength = 6)]
    public string Password { get; set; } = null!;
}
