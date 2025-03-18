using System.ComponentModel.DataAnnotations;

namespace Common.Dto;

public class AuthenticateDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 6)]
    public string Password { get; set; }
}
