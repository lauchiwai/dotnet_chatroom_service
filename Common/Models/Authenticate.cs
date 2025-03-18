using Microsoft.EntityFrameworkCore;
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
    [Unicode(false)]
    public string UserName { get; set; } = null!;

    [Column("pw")]
    [StringLength(100)]
    [Unicode(false)]
    public string Pw { get; set; } = null!;

    [Column("refresh_token")]
    [StringLength(50)]
    [Unicode(false)]
    public string? RefreshToken { get; set; }

    [Column("refresh_token_expiry_time", TypeName = "datetime")]
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
