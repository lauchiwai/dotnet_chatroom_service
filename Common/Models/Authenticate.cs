using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models;

[Table("Authenticate")]
public partial class Authenticate
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_name")]
    [StringLength(50)]
    public string? UserName { get; set; }

    [Column("pw")]
    [StringLength(50)]
    public string? Pw { get; set; }

    [Column("refresh_token")]
    [StringLength(50)]
    public string? RefreshToken { get; set; }

    [Column("refresh_token_expiry_time")]
    public DateOnly? RefreshTokenExpiryTime { get; set; }
}
