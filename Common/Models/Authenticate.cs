using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models;

[Table("Authenticate")]
public partial class Authenticate
{
    /// <summary>
    /// 使用者編號
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 使用者名稱
    /// </summary>
    [Column("user_name")]
    [StringLength(50)]
    [Unicode(false)]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// pw
    /// </summary>
    [Column("pw")]
    [StringLength(100)]
    [Unicode(false)]
    public string Pw { get; set; } = null!;

    /// <summary>
    /// refresh_token
    /// </summary>
    [Column("refresh_token")]
    [StringLength(50)]
    [Unicode(false)]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// refresh_token 過期時間
    /// </summary>
    [Column("refresh_token_expiry_time", TypeName = "datetime")]
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
